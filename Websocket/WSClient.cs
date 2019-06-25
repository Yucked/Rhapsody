using Frostbyte.Handlers;
using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.WebSockets;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Frostbyte.Entities.Packets;
using Frostbyte.Entities.Enums;

namespace Frostbyte.Websocket
{
    public sealed class WSClient : IAsyncDisposable
    {
        private readonly IPEndPoint _endPoint;
        private ReadyPacket readyPacket;

        public readonly WebSocket _socket;
        public event Func<IPEndPoint, Task> OnClosed;
        public ConcurrentDictionary<ulong, WSVoiceClient> VoiceClients { get; private set; }

        public WSClient(WebSocketContext socketContext, IPEndPoint endPoint)
        {
            _socket = socketContext.WebSocket;
            _endPoint = endPoint;
            VoiceClients = new ConcurrentDictionary<ulong, WSVoiceClient>();
        }

        public async Task ReceiveAsync(CancellationTokenSource cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested && _socket.State == WebSocketState.Open)
                {
                    var memory = new Memory<byte>();
                    var result = await _socket.ReceiveAsync(memory, CancellationToken.None).ConfigureAwait(false);
                    switch (result.MessageType)
                    {
                        case WebSocketMessageType.Close:
                            OnClosed?.Invoke(_endPoint);
                            break;

                        case WebSocketMessageType.Text:
                            var packet = JsonSerializer.Parse<PlayerPacket>(memory.Span);
                            await ProcessPacketAsync(packet).ConfigureAwait(false);
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                LogHandler<WSClient>.Log.Error(ex?.InnerException ?? ex);
                OnClosed?.Invoke(_endPoint);
            }
            finally
            {
                await DisposeAsync().ConfigureAwait(false);
                OnClosed?.Invoke(_endPoint);
            }
        }

        private async Task ProcessPacketAsync(PlayerPacket packet)
        {
            if (readyPacket is null && !(packet is ReadyPacket))
            {
                LogHandler<WSClient>.Log.RawLog(LogLevel.Critical,
                    $"{packet.GuildId} guild didn't send a ReadyPayload. All functions are disabled.", default);
                return;
            }
            else
            {
                readyPacket = packet as ReadyPacket;
                var vClient = new WSVoiceClient(packet.GuildId, _socket);
                vClient.Engine.ToggleCrossfade = readyPacket.ToggleCrossfade;
                VoiceClients.TryAdd(packet.GuildId, vClient);

                LogHandler<WSClient>.Log.Debug($"{packet.GuildId} client and engine has been initialized.");
            }

            var voiceClient = VoiceClients[packet.GuildId];

            switch (packet)
            {
                case PlayPacket play:
                    await voiceClient.Engine.PlayAsync(play).ConfigureAwait(false);
                    break;

                case PausePacket pause:
                    voiceClient.Engine.Pause(pause);
                    break;

                case StopPacket stop:
                    voiceClient.Engine.Stop(stop);
                    break;

                case DestroyPacket destroy:
                    await voiceClient.Engine.DisposeAsync().ConfigureAwait(false);
                    break;

                case SeekPacket seek:
                    voiceClient.Engine.Seek(seek);
                    break;

                case EqualizerPacket equalizer:
                    await voiceClient.Engine.EqualizeAsync().ConfigureAwait(false);
                    break;

                case VoiceUpdatePacket voiceUpdate:
                    if (string.IsNullOrWhiteSpace(voiceUpdate.EndPoint))
                        return;

                    await VoiceClients[packet.GuildId].HandleVoiceUpdateAsync(voiceUpdate).ConfigureAwait(false);
                    break;
            }
        }

        public async ValueTask DisposeAsync()
        {
            foreach (var (key, value) in VoiceClients)
            {
                await value.DisposeAsync().ConfigureAwait(false);
                VoiceClients.TryRemove(key, out _);
            }

            VoiceClients.Clear();
            VoiceClients = null;

            await _socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Disposing client.", CancellationToken.None).ConfigureAwait(false);
            _socket.Dispose();
        }
    }
}