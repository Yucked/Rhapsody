using System.Collections.Concurrent;
using System.Net.WebSockets;
using Vysn.Voice;

namespace Concept.Options
{
    public sealed class ClientOptions
    {
        public ulong UserId { get; set; }
        public WebSocket Socket { get; set; }
        public ConcurrentDictionary<ulong, VoiceGatewayClient> GatewayClients { get; }

        public ClientOptions()
        {
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