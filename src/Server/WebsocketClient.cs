using System;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Frostbyte.Entities.Enums;
using Frostbyte.Entities.Payloads;
using Frostbyte.Factories;

namespace Frostbyte.Server
{
    public sealed class WebsocketClient : IAsyncDisposable
    {
        public bool IsConnected
            => Volatile.Read(ref _isConnected);

        public ConcurrentDictionary<ulong, WebsocketVoice> Voices { get; }
        private readonly ushort _buffer;
        private readonly WebSocketContext _context;
        private readonly WebSocket _socket;
        private readonly CancellationTokenSource _source;
        private readonly SourceFactory _sourceFactory;

        private readonly ulong _userId;
        private bool _isConnected;

        public WebsocketClient(WebSocketContext webSocketContext, ulong userId, ushort buffer)
        {
            Volatile.Write(ref _isConnected, true);
            _context = webSocketContext;
            _userId = userId;
            _buffer = buffer;
            _socket = webSocketContext.WebSocket;
            _source = new CancellationTokenSource();
            Voices = new ConcurrentDictionary<ulong, WebsocketVoice>();
            _sourceFactory = Singleton.Of<SourceFactory>();
        }

        public async ValueTask DisposeAsync()
        {
            foreach (var (guildId, voice) in Voices)
            {
                await voice.DisposeAsync()
                    .ConfigureAwait(false);

                LogFactory.Debug<WebsocketClient>($"Disposed voice ws connection for {guildId}.");
            }

            Voices.Clear();
            _source?.Cancel(false);
            _context.WebSocket.Dispose();
            Volatile.Write(ref _isConnected, false);
        }

        public async Task CloseAsync(string reason)
        {
            _source.Cancel(false);
            await _socket.CloseAsync(WebSocketCloseStatus.NormalClosure, reason, CancellationToken.None)
                .ConfigureAwait(false);
        }

        public async Task SendAsync<T>(T value)
        {
            await _socket.SendAsync(value)
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
            var voice = GetConnection(payload.GuildId);

            switch (payload.Op)
            {
                case OperationType.VoiceServer:
                    var serverPayload = bytes.Deserialize<VoiceServerPayload>();
                    await voice.ProcessVoiceServerPaylaodAsync(serverPayload)
                        .ConfigureAwait(false);
                    break;

                case OperationType.Play:
                    var playPayload = bytes.Deserialize<PlayPayload>();
                    await HandlePlayAsync(playPayload)
                        .ConfigureAwait(false);
                    break;
            }
        }

        private async Task HandlePlayAsync(PlayPayload playPayload)
        {
            var track = _sourceFactory.GetTrack(playPayload.TrackId);
            if (playPayload.StartTime < 0)
            {
                LogFactory.Error<WebsocketClient>($"Guild {playPayload.GuildId}: Out of range startime.");
                return;
            }

            if (playPayload.EndTime > track.Duration)
            {
                LogFactory.Error<WebsocketClient>(
                    $"Guild {playPayload.GuildId}: Provided endtime is greater than track's duration.");
                return;
            }

            var voice = GetConnection(playPayload.GuildId);
            if (!voice.IsConnected)
            {
                LogFactory.Error<WebsocketClient>($"Guild {playPayload.GuildId}: Voice connection isn't ready.");
                return;
            }

            var stream = await _sourceFactory.GetStreamAsync(track.Provider, track.Id)
                .ConfigureAwait(false);

            if (stream.Length is 0)
            {
                LogFactory.Error<WebsocketClient>(
                    $"Guild {playPayload.GuildId}: {playPayload.TrackId} returned an invalid stream.");
                return;
            }

            await voice.Player.PlayAsync(stream, voice.AudioStream)
                .ConfigureAwait(false);
        }

        private WebsocketVoice GetConnection(ulong guildId)
        {
            if (!Voices.TryGetValue(guildId, out var voice))
                voice = new WebsocketVoice(_userId);

            Voices.AddOrUpdate(guildId, voice, (id, websocketVoice) => voice);
            return voice;
        }
    }
}