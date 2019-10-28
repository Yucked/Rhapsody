using System.Collections.Concurrent;
using System.Net.WebSockets;
using Vysn.Voice;

namespace Concept.Entities
{
    public readonly struct SocketConnection
    {
        public ulong UserId { get; }
        public WebSocket Socket { get; }
        public ConcurrentDictionary<ulong, VoiceGatewayClient> GatewayClients { get; }

        public SocketConnection(ulong userId, WebSocket socket)
        {
            UserId = userId;
            Socket = socket;
            GatewayClients = new ConcurrentDictionary<ulong, VoiceGatewayClient>();
        }

        public VoiceGatewayClient GetGatewayClient(ulong guildId)
            => GatewayClients.TryGetValue(guildId, out var gatewayClient) ? gatewayClient : default;

        public void AddGatewayClient(VoiceGatewayClient gatewayClient)
            => GatewayClients.TryAdd(gatewayClient.GuildId.Raw, gatewayClient);

        public void RemoveClient(ulong guildId)
        {
            GatewayClients.TryGetValue(guildId, out var gatewayClient);
            gatewayClient.DisposeAsync();
            GatewayClients.TryRemove(guildId, out gatewayClient);
        }
    }
}