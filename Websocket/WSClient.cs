using Frostbyte.Entities.Operations;
using Frostbyte.Handlers;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Utf8;
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
        private readonly IPEndPoint _endPoint;
        private readonly HttpListenerWebSocketContext _wsContext;
        private readonly LogHandler<WsClient> _log;
        public readonly ConcurrentDictionary<ulong, GuildHandler> Guilds;

        public WsClient(HttpListenerWebSocketContext socketContext, ulong userId, int shards, IPEndPoint endPoint)
        {
            _wsContext = socketContext;
            _socket = socketContext.WebSocket;
            _userId = userId;
            _shards = shards;
            _endPoint = endPoint;
            _utf8 = new UTF8Encoding(false);
            _log = new LogHandler<WsClient>();
            Guilds = new ConcurrentDictionary<ulong, GuildHandler>();
        }

        public async ValueTask DisposeAsync()
        {
            await _socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Disposing client.", CancellationToken.None).ConfigureAwait(false);
            _socket.Dispose();
        }

        public event Func<IPEndPoint, ulong, Task> OnClosed;

        public async Task ReceiveAsync(CancellationTokenSource cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested && _socket.State == WebSocketState.Open)
                {
                    var memory = new Memory<byte>();
                    var result = await _socket.ReceiveAsync(memory, CancellationToken.None).ConfigureAwait(false);
                    var data = string.Empty;

                    switch (result.MessageType)
                    {
                        case WebSocketMessageType.Binary:
                            data = _utf8.GetString(memory.Span);
                            break;

                        case WebSocketMessageType.Close:
                            OnClosed?.Invoke(_endPoint, _userId);
                            break;

                        case WebSocketMessageType.Text:
                            data = _utf8.GetString(memory.Span);
                            var parse = JsonSerializer.Parse<FrostOp>(data);
                            var guild = Guilds[parse.GuildId] ??= new GuildHandler();
                            guild.OnMessage?.Invoke(parse);
                            break;
                    }


                }
            }
            catch
            {
                OnClosed?.Invoke(_endPoint, _userId);
            }
            finally
            {

            }
        }

        public async Task SendAsync(object data)
        {
            var bytes = JsonSerializer.ToBytes(data);
            await _socket.SendAsync(bytes, WebSocketMessageType.Text, true, CancellationToken.None);
            _log.LogDebug($"Sent {bytes.Length} bytes to {_endPoint}.");
        }
    }
}