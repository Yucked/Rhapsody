using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace Concept.WebSockets
{
    public class WebSocketConnectionManager
    {
        private ConcurrentDictionary<string, WebSocket> _sockets = new ConcurrentDictionary<string, WebSocket>();

        public WebSocket GetSocketById(string id)
            => _sockets.FirstOrDefault(p => p.Key == id).Value;

        public ConcurrentDictionary<string, WebSocket> GetAll()
            => _sockets;

        public string GetId(WebSocket socket)
            => _sockets.FirstOrDefault(p => p.Value == socket).Key;

        public void AddSocket(WebSocket socket)
            => _sockets.TryAdd(CreateConnectionId(), socket);

        public async Task RemoveSocket(string id)
        {
            _sockets.TryRemove(id, out var socket);

            await socket.CloseAsync(closeStatus: WebSocketCloseStatus.NormalClosure,
                                    statusDescription: "Closed by the WebSocketManager",
                                    cancellationToken: CancellationToken.None);
        }

        private string CreateConnectionId()
            => Guid.NewGuid().ToString();
    }
}