using System.Collections.Concurrent;
using Concept.Entities;

namespace Concept.Caches
{
    public sealed class ClientsCache
    {
        public int Count
            => Clients.Count;

        public ConcurrentDictionary<ulong, SocketConnection> Clients { get; }

        public ClientsCache()
            => Clients = new ConcurrentDictionary<ulong, SocketConnection>();

        public SocketConnection GetConnection(ulong snowflake)
            => Clients.TryGetValue(snowflake, out var client) ? client : default;

        public void AddConnection(SocketConnection connection)
            => Clients.TryAdd(connection.UserId, connection);

        public void RemoveConnection(ulong snowflake)
        {
            Clients.TryGetValue(snowflake, out var options);
            options.Socket.Dispose();
            options.GatewayClients.Clear();
            Clients.TryRemove(snowflake, out options);
        }
    }
}