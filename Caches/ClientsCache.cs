using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.WebSockets;

namespace Concept.Caches
{
    public sealed class ClientsCache
    {
        private readonly ConcurrentDictionary<ulong, WebSocket> _clients;

        public ClientsCache()
            => _clients = new ConcurrentDictionary<ulong, WebSocket>();

        public WebSocket GetClient(ulong snowflake)
            => _clients.TryGetValue(snowflake, out var client) ? client : default;

        public IReadOnlyDictionary<ulong, WebSocket> GetAll()
            => _clients;

        public void AddClient(ulong snowflake, WebSocket socket)
            => _clients.TryAdd(snowflake, socket);

        public void RemoveClient(ulong snowflake)
        {
            _clients.TryGetValue(snowflake, out var socket);
            socket.Dispose();
            _clients.TryRemove(snowflake, out socket);
        }
    }
}