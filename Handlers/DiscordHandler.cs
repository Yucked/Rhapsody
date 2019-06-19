using Frostbyte.Entities.Discord;
using Frostbyte.Entities.Packets;
using Frostbyte.Extensions;
using System;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Frostbyte.Handlers
{
    public sealed class DiscordHandler : IAsyncDisposable
    {
        private ClientWebSocket _socket;
        private CancellationTokenSource _receiveCancel, _heartBeatCancel;
        private SessionDescriptionPayload _sdp;
        private Task _receiveTask, _heartBeatTask;
        private UdpClient _udp;
        private VoiceReadyPayload _vrp;

        public async Task HandleVoiceUpdateAsync(VoiceUpdatePacket voiceUpdate)
        {
            if (_socket != null && _socket.State != WebSocketState.None && _socket.State != WebSocketState.Closed)
            {
                await _socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Restarting.", CancellationToken.None)
                             .ConfigureAwait(false);
            }

            _socket = new ClientWebSocket();
            try
            {
                await _socket.ConnectAsync(new Uri($"wss://{voiceUpdate.EndPoint}/?v=3"), CancellationToken.None)
                             .ContinueWith(x => VerifyConnectionAsync(voiceUpdate, x))
                             .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                LogHandler<DiscordHandler>.Log.Error(ex?.InnerException ?? ex);
            }
        }

        private async Task VerifyConnectionAsync(VoiceUpdatePacket packet, Task task)
        {
            if (task.IsCanceled || task.IsFaulted || task.Exception != null)
            {
                LogHandler<DiscordHandler>.Log.Error(task.Exception);
            }
            else
            {
                _receiveCancel ??= new CancellationTokenSource();
                _receiveTask = Task.Run(ReceiveTaskAsync, _receiveCancel.Token);
                var payload = new BaseDiscordPayload(0, new IdentifyPayload(packet.GuildId, packet.UserId, packet.SessionId, packet.Token));
                await _socket.SendAsync(payload).ConfigureAwait(false);
            }
        }

        private async Task ReceiveTaskAsync()
        {
            try
            {
                while (!_receiveCancel.IsCancellationRequested && _socket.State == WebSocketState.Open)
                {
                    var memory = new Memory<byte>();
                    var result = await _socket.ReceiveAsync(memory, CancellationToken.None).ConfigureAwait(false);
                    switch (result.MessageType)
                    {
                        case WebSocketMessageType.Close:
                            await _socket.CloseAsync(WebSocketCloseStatus.NormalClosure, String.Empty, CancellationToken.None)
                                         .ConfigureAwait(false);
                            break;

                        case WebSocketMessageType.Text:
                            var parse = JsonSerializer.Parse<BaseDiscordPayload>(memory.Span);
                            await ProcessPayloadAsync(parse).ConfigureAwait(false);
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                LogHandler<DiscordHandler>.Log.Error(ex?.InnerException ?? ex);
            }
            finally
            {
                _socket?.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None).ConfigureAwait(false);
                _socket?.Dispose();
            }
        }

        private async Task ProcessPayloadAsync(BaseDiscordPayload payload)
        {
            switch (payload.OpCode)
            {
                case 2:
                    _vrp = payload.Data.TryCast<VoiceReadyPayload>();
                    var basePayload = new BaseDiscordPayload(1, new SelectPayload(_vrp.IPAddress, _vrp.Port));
                    await _socket.SendAsync(basePayload);
                    break;

                case 4:
                    _sdp = payload.Data.TryCast<SessionDescriptionPayload>();
                    if (_sdp.Mode != "xsalsa20_poly1305")
                        return;

                    try
                    {
                        _udp = new UdpClient();
                        _udp.Connect(_vrp.IPAddress, _vrp.Port);
                        await _udp.ProcessVoiceReadyAsync(_vrp).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        LogHandler<DiscordHandler>.Log.Error(ex?.InnerException ?? ex);
                    }
                    break;

                case 8:
                    var helloPayload = payload.Data.TryCast<HelloPayload>();
                    if (_heartBeatTask != null)
                    {
                        _heartBeatCancel.Cancel(false);
                        _heartBeatTask.Dispose();
                        _heartBeatTask = null;
                    }

                    _heartBeatCancel ??= new CancellationTokenSource();
                    _heartBeatTask = Task.Run(() => HandleHeartbeatAsync(helloPayload.HeartBeatInterval), _heartBeatCancel.Token);
                    break;

                default:
                    LogHandler<DiscordHandler>.Log.Debug($"Received {payload.OpCode} op code.");
                    break;
            }
        }

        private async Task HandleHeartbeatAsync(int interval)
        {
            while (!_heartBeatCancel.IsCancellationRequested)
            {
                await Task.Delay(interval).ConfigureAwait(false);
                var payload = new BaseDiscordPayload(3, DateTimeOffset.UtcNow.ToUnixTimeSeconds());
                await _socket.SendAsync(payload).ConfigureAwait(false);
            }
        }

        public async ValueTask DisposeAsync()
        {
            _heartBeatCancel.Cancel(false);
            _receiveCancel.Cancel(true);
            _heartBeatTask?.Dispose();
            _receiveTask?.Dispose();
        }
    }
}