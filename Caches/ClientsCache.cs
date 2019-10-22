using System.Collections.Concurrent;
using System.Collections.Generic;
using Concept.Options;

namespace Concept.Caches
{
    public sealed class ClientsCache
    {
        private readonly ConcurrentDictionary<ulong, ClientOptions> _clients;

        public ClientsCache()
            => _clients = new ConcurrentDictionary<ulong, ClientOptions>();

        public ClientOptions GetClient(ulong snowflake)
            => _clients.TryGetValue(snowflake, out var client) ? client : default;

        public IReadOnlyDictionary<ulong, ClientOptions> GetAll()
            => _clients;

        public void AddClient(ClientOptions options)
            => _clients.TryAdd(options.UserId, options);

        public void RemoveClient(ulong snowflake)
        {
            _clients.TryGetValue(snowflake, out var options);
            options.Socket.Dispose();
            options.GatewayClients.Clear();
            _clients.TryRemove(snowflake, out options);
        }
    }
}