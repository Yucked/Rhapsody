using Frostbyte.Handlers;
using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.WebSockets;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Frostbyte.Entities.Packets;
using Frostbyte.Audio;

namespace Frostbyte.Websocket
{
    public sealed class WsClient : IAsyncDisposable
    {
        private readonly ulong _userId;
        private readonly IPEndPoint _endPoint;

        public readonly WebSocket _socket;
        public event Func<IPEndPoint, Task> OnClosed;
        public ConcurrentDictionary<ulong, DiscordHandler> Handlers { get; private set; }

        public WsClient(WebSocketContext socketContext, IPEndPoint endPoint)
        {
            _socket = socketContext.WebSocket;
            _endPoint = endPoint;
            Handlers = new ConcurrentDictionary<ulong, DiscordHandler>();
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
                LogHandler<WsClient>.Log.Error(ex?.InnerException ?? ex);
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
            var handler = GetHandler(packet.GuildId, out var isNew);
            if (isNew && !(packet is VoiceUpdatePacket))
                return;

            switch (packet)
            {
                case PlayPacket play:
                    await handler.PlaybackEngine.PlayAsync(play).ConfigureAwait(false);
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

                    await handler.HandleVoiceUpdateAsync(voiceUpdate).ConfigureAwait(false);
                    break;
            }
        }

        private DiscordHandler GetHandler(ulong guildId, out bool isNew)
        {
            if (Handlers.TryGetValue(guildId, out var handler))
            {
                isNew = false;
                return handler;
            }

            handler = new DiscordHandler();
            isNew = true;
            return handler;
        }

        public async ValueTask DisposeAsync()
        {
            foreach (var (key, value) in Handlers)
            {
                await value.DisposeAsync().ConfigureAwait(false);
                Handlers.TryRemove(key, out _);
            }
            Handlers.Clear();
            Handlers = null;
            await _socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Disposing client.", CancellationToken.None).ConfigureAwait(false);
            _socket.Dispose();
        }
    }
}