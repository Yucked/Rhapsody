using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.WebSockets;

namespace Concept.WebSockets
{
    public sealed class ClientsCache
    {
        private readonly ConcurrentDictionary<IPAddress, WebSocket> _clients;

        public ClientsCache()
            => _clients = new ConcurrentDictionary<IPAddress, WebSocket>();

        public WebSocket GetClient(IPAddress address)
            => _clients.TryGetValue(address, out var client) ? client : default;

        public IReadOnlyDictionary<IPAddress, WebSocket> GetAll()
            => _clients;

        public void AddClient(IPAddress address, WebSocket socket)
            => _clients.TryAdd(address, socket);

        public void RemoveClient(IPAddress address)
        {
            _clients.TryGetValue(address, out var socket);
            socket.Dispose();
            _clients.TryRemove(address, out socket);
        }
    }
}