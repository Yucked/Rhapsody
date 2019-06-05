using System;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Frostbyte.Entities.Discord;
using Frostbyte.Entities.Packets;
using Frostbyte.Extensions;

namespace Frostbyte.Handlers
{
    public sealed class GuildHandler : IAsyncDisposable
    {
        public bool IsPlaying { get; private set; }
        public event Func<ulong, bool> OnClosed;

        private readonly int _shards;
        private readonly ulong _guildId, _userId;


        private ClientWebSocket _socket;
        private CancellationTokenSource _mainToken, _heartBeatToken, _receiveToken;
        private Task _receiveTask, _heartBeatTask;
        private UdpClient UdpClient;
        private VoiceReadyPayload VoiceReadyPayload;

        public GuildHandler(ulong guildId, ulong userId, int shards)
        {
            _shards = shards;
            _guildId = guildId;
            _userId = userId;
            _heartBeatToken = new CancellationTokenSource();
            _receiveToken = new CancellationTokenSource();
            _mainToken = CancellationTokenSource.CreateLinkedTokenSource(_heartBeatToken.Token, _receiveToken.Token);
        }

        public async Task HandlePacketAsync(PlayerPacket packet)
        {
            switch (packet)
            {
                case PlayPacket play:
                    break;

                case PausePacket pause:
                    break;

                case StopPacket stop:
                    break;

                case DestroyPacket destroy:
                    break;

                case SeekPacket seek:
                    break;

                case EqualizerPacket equalizer:
                    break;

                case VoiceUpdatePacket voiceUpdate:
                    if (string.IsNullOrWhiteSpace(voiceUpdate.EndPoint))
                        return;

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
                        LogHandler<GuildHandler>.Log.Error(ex);
                    }

                    break;
            }
        }

        private async Task VerifyConnectionAsync(VoiceUpdatePacket packet, Task task)
        {
            if (task.IsCanceled || task.IsFaulted || task.Exception != null)
            {
                LogHandler<GuildHandler>.Log.Error(task.Exception);
            }
            else
            {
                _receiveToken ??= new CancellationTokenSource();
                _receiveTask = Task.Run(ReceiveTaskAsync, _receiveToken.Token);
                var payload = new BasePayload(0, new IdentifyPayload(_guildId, _userId, packet.SessionId, packet.Token));
                await _socket.SendAsync<GuildHandler>(payload).ConfigureAwait(false);
            }
        }

        private async Task ReceiveTaskAsync()
        {
            try
            {
                while (!_receiveToken.IsCancellationRequested && _socket.State == WebSocketState.Open)
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
                            var parse = JsonSerializer.Parse<BasePayload>(memory.Span);
                            await HandleResponseAsync(parse);
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                LogHandler<GuildHandler>.Log.Error(ex);
            }
            finally
            {
                _socket?.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None).ConfigureAwait(false);
                _socket?.Dispose();
            }
        }

        private async Task HandleResponseAsync(BasePayload payload)
        {
            switch (payload.OpCode)
            {
                case 2:
                    VoiceReadyPayload = payload.Data.TryCast<VoiceReadyPayload>();
                    var basePayload = new BasePayload(1, new SelectPayload(VoiceReadyPayload.IPAddress, VoiceReadyPayload.Port));
                    await _socket.SendAsync<GuildHandler>(basePayload);
                    break;

                case 4:
                    var sessionDescription = payload.Data.TryCast<SessionDescriptionPayload>();
                    if (sessionDescription.Mode != "xsalsa20_poly1305")
                        return;

                    try
                    {
                        UdpClient = new UdpClient();
                        UdpClient.Connect(VoiceReadyPayload.IPAddress, VoiceReadyPayload.Port);
                        await HandleUdpAsync().ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        LogHandler<GuildHandler>.Log.Error(ex);
                    }

                    break;

                case 8:
                    var helloPayload = payload.Data.TryCast<HelloPayload>();
                    if (_heartBeatTask != null)
                    {
                        _heartBeatToken.Cancel(false);
                        _heartBeatTask.Dispose();
                        _heartBeatTask = null;
                    }

                    _heartBeatToken = new CancellationTokenSource();
                    _heartBeatTask = Task.Run(() => HandleHeartbeatAsync(helloPayload.HeartBeatInterval), _heartBeatToken.Token);
                    break;

                default:
                    LogHandler<GuildHandler>.Log.Debug($"Received {payload.OpCode} op code.");
                    break;
            }
        }

        private async Task HandleHeartbeatAsync(int interval)
        {
            while (!_heartBeatToken.IsCancellationRequested)
            {
                await Task.Delay(interval).ConfigureAwait(false);
                var payload = new BasePayload(3, DateTimeOffset.UtcNow.ToUnixTimeSeconds());
                await _socket.SendAsync<GuildHandler>(payload).ConfigureAwait(false);
            }
        }

        private async Task HandleUdpAsync()
        {
            var packet = new byte[70];
            packet[0] = (byte)(VoiceReadyPayload.SSRC >> 24);
            packet[1] = (byte)(VoiceReadyPayload.SSRC >> 16);
            packet[2] = (byte)(VoiceReadyPayload.SSRC >> 8);
            packet[3] = (byte)(VoiceReadyPayload.SSRC >> 0);
            await SendUdpPacketAsync(packet).ConfigureAwait(false);
        }

        private Task SendUdpPacketAsync(byte[] data)
        {
            return UdpClient.SendAsync(data, data.Length);
        }

        public async ValueTask DisposeAsync()
        {
            UdpClient.Close();
            UdpClient.Dispose();
            _mainToken.Cancel(false);
            _heartBeatTask.Dispose();
            _receiveTask.Dispose();
        }
    }
}