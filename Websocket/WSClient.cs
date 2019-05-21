using System;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Frostbyte.Websocket
{
    public sealed class WsClient : IAsyncDisposable
    {
        private readonly int _shards;
        private readonly WebSocket _socket;
        private readonly ulong _userId;
        private readonly Encoding _utf8;
        private readonly HttpListenerWebSocketContext _wsContext;
        public readonly ConcurrentDictionary<ulong, object> GuildConnections;

        public WsClient(HttpListenerWebSocketContext socketContext, ulong userId, int shards)
        {
            _wsContext = socketContext;
            _socket = socketContext.WebSocket;
            _userId = userId;
            _shards = shards;
            _utf8 = new UTF8Encoding(false);
        }

        public async ValueTask DisposeAsync()
        {
            await _socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Disposing client.", CancellationToken.None).ConfigureAwait(false);
            _socket.Dispose();
        }

        public event Func<ulong, Task> OnClosed;

        public async Task ReceiveAsync(CancellationTokenSource cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
                return;
        }

        public async Task SendAsync(ReadOnlyMemory<byte> bytes)
        {
            await _socket.SendAsync(bytes, WebSocketMessageType.Text, true, CancellationToken.None);
        }
    }
}