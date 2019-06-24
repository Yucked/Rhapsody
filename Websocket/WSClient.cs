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
using Frostbyte.Entities.Enums;

namespace Frostbyte.Websocket
{
    public sealed class WSClient : IAsyncDisposable
    {
        private readonly ulong _userId;
        private readonly IPEndPoint _endPoint;
        private BasePacket firstPacket;

        public readonly WebSocket _socket;
        public event Func<IPEndPoint, Task> OnClosed;
        public ConcurrentDictionary<ulong, WSVoiceClient> VoiceClients { get; private set; }
        public ConcurrentDictionary<ulong, PlaybackEngine> Engines { get; private set; }

        public WSClient(WebSocketContext socketContext, IPEndPoint endPoint)
        {
            _socket = socketContext.WebSocket;
            _endPoint = endPoint;
            VoiceClients = new ConcurrentDictionary<ulong, WSVoiceClient>();
            Engines = new ConcurrentDictionary<ulong, PlaybackEngine>();
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
            if (firstPacket is null && !(packet is ReadyPacket))
            {
                LogHandler<WSClient>.Log.RawLog(LogLevel.Critical,
                    $"{packet.GuildId} guild didn't send a ready packet. PlaybackEngine endpoints won't function.",
                    default);
                return;
            }


            Engines.TryGetValue(packet.GuildId, out var engine);

            switch (packet)
            {
                case ReadyPacket ready:
                    if (!Engines.ContainsKey(packet.GuildId))
                        Engines.TryAdd(packet.GuildId, new PlaybackEngine(_socket, true, ready.ToggleCrossfade));

                    if (!VoiceClients.ContainsKey(packet.GuildId))
                        VoiceClients.TryAdd(packet.GuildId, new WSVoiceClient());
                    break;

                case PlayPacket play:
                    await engine.PlayAsync(play).ConfigureAwait(false);
                    break;

                case PausePacket pause:
                    engine.Pause(pause);
                    break;

                case StopPacket stop:
                    engine.Stop(stop);
                    break;

                case DestroyPacket destroy:
                    await engine.DisposeAsync().ConfigureAwait(false);
                    break;

                case SeekPacket seek:
                    engine.Seek(seek);
                    break;

                case EqualizerPacket equalizer:
                    await engine.EqualizeAsync().ConfigureAwait(false);
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

            foreach (var (key, value) in Engines)
            {
                value.Stop(default);
                await value.DisposeAsync().ConfigureAwait(false);
            }

            VoiceClients.Clear();
            VoiceClients = null;
            Engines.Clear();
            Engines = null;

            await _socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Disposing client.", CancellationToken.None).ConfigureAwait(false);
            _socket.Dispose();
        }
    }
}