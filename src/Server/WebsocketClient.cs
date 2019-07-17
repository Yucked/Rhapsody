using System;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Frostbyte.AudioEngine;
using Frostbyte.Entities.Enums;
using Frostbyte.Entities.Payloads;
using Frostbyte.Factories;
using Frostbyte.Misc;

namespace Frostbyte.Server
{
    public sealed class WebsocketClient : IAsyncDisposable
    {
        public bool IsDisposed { get; private set; }
        private readonly ushort _buffer;
        private readonly ConcurrentDictionary<ulong, WebsocketVoice> _voices;
        private readonly ConcurrentDictionary<ulong, AudioPlayer> _players;
        private readonly WebSocketContext _context;
        private readonly WebSocket _socket;
        private readonly CancellationTokenSource _source;

        private readonly ulong _userId;

        public WebsocketClient(WebSocketContext webSocketContext, ulong userId, ushort buffer)
        {
            _context = webSocketContext;
            _userId = userId;
            _buffer = buffer;
            _socket = webSocketContext.WebSocket;
            _source = new CancellationTokenSource();
            _voices = new ConcurrentDictionary<ulong, WebsocketVoice>();
            _players = new ConcurrentDictionary<ulong, AudioPlayer>();
        }

        public ValueTask DisposeAsync()
        {
            IsDisposed = true;
            _source?.Cancel(false);
            _context.WebSocket.Dispose();
            return default;
        }

        public async Task CloseAsync(string reason)
        {
            _source.Cancel(false);
            await _socket.CloseAsync(WebSocketCloseStatus.NormalClosure, reason, CancellationToken.None)
                .ConfigureAwait(false);
        }

        public async Task ReceiveAsync()
        {
            while (!_source.IsCancellationRequested && _socket.State == WebSocketState.Open)
                try
                {
                    var bytes = new byte[_buffer];
                    var memory = new Memory<byte>(bytes);
                    var result = await _socket.ReceiveAsync(memory, default)
                        .ConfigureAwait(false);

                    switch (result.MessageType)
                    {
                        case WebSocketMessageType.Close:
                            await DisposeAsync()
                                .ConfigureAwait(false);
                            break;

                        case WebSocketMessageType.Text:
                            if (!result.EndOfMessage)
                                continue;

                            Extensions.TrimEnd(ref bytes);
                            await ProcessPayloadAsync(bytes)
                                .ConfigureAwait(false);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    LogFactory.Error<WebsocketClient>($"Client with {_userId} threw an exception.", ex);
                    await DisposeAsync()
                        .ConfigureAwait(false);
                }
        }

        private async Task ProcessPayloadAsync(ReadOnlyMemory<byte> bytes)
        {
            if (bytes.IsEmpty)
                return;

            LogFactory.Debug<WebsocketClient>(bytes.GetString());

            var payload = bytes.Deserialize<BasePayload>();
            var (player, voice) = GetConnection(payload.GuildId);

            switch (payload.Op)
            {
                case OperationType.VoiceServer:
                    var serverPayload = bytes.Deserialize<VoiceServerPayload>();
                    await voice.ProcessVoiceServerPaylaodAsync(serverPayload)
                        .ConfigureAwait(false);
                    break;

                case OperationType.Play:
                    var playPayload = bytes.Deserialize<PlayPayload>();
                    await player.PlayAsync(playPayload)
                        .ConfigureAwait(false);
                    break;
            }
        }

        private (AudioPlayer player, WebsocketVoice voice) GetConnection(ulong guildId)
        {
            if (!_voices.TryGetValue(guildId, out var voice))
                voice = new WebsocketVoice(_userId);

            if (!_players.TryGetValue(guildId, out var player))
                player = new AudioPlayer();

            _voices.AddOrUpdate(guildId, voice, (id, websocketVoice) => voice);
            _players.AddOrUpdate(guildId, player, (id, audioPlayer) => player);

            return (player, voice);
        }
    }
}