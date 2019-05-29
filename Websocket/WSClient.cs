using Frostbyte.Handlers;
using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.WebSockets;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Frostbyte.Entities.Packets;
using System.Text.Utf8;

namespace Frostbyte.Websocket
{
    public sealed class WsClient : IAsyncDisposable
    {
        private readonly int _shards;
        private readonly WebSocket _socket;
        private readonly ulong _userId;
        private readonly IPEndPoint _endPoint;
        private readonly LogHandler<WsClient> _log;

        public event Func<IPEndPoint, ulong, Task> OnClosed;
        public ConcurrentDictionary<ulong, GuildHandler> Guilds { get; }

        public WsClient(WebSocketContext socketContext, ulong userId, int shards, IPEndPoint endPoint)
        {
            _socket = socketContext.WebSocket;
            _userId = userId;
            _shards = shards;
            _endPoint = endPoint;
            _log = new LogHandler<WsClient>();
            Guilds = new ConcurrentDictionary<ulong, GuildHandler>();
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
                            OnClosed?.Invoke(_endPoint, _userId);
                            break;

                        case WebSocketMessageType.Text:
                            var packet = JsonSerializer.Parse<PlayerPacket>(memory.Span);
                            var guild = Guilds[packet.GuildId] ??= new GuildHandler(_userId, _shards);
                            await guild.HandlePacketAsync(packet).ConfigureAwait(false);
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                OnClosed?.Invoke(_endPoint, _userId);
                _log.LogError(ex);
            }
            finally
            {
                await DisposeAsync().ConfigureAwait(false);
                OnClosed?.Invoke(_endPoint, _userId);
            }
        }

        public async Task SendAsync(ReadOnlyMemory<byte> bytes)
        {
            await _socket.SendAsync(bytes, WebSocketMessageType.Text, true, CancellationToken.None);
            var str = new Utf8String(bytes.Span);
            _log.LogDebug(str.ToString());
        }

        public async ValueTask DisposeAsync()
        {
            await _socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Disposing client.", CancellationToken.None).ConfigureAwait(false);
            _socket.Dispose();
        }
    }
}