using System.Text.Json.Serialization;

namespace Concept.Entities.Payloads
{
    public class BasePayload
    {
        [JsonPropertyName("op")]
        public PayloadType PayloadType { get; set; }
        
        [JsonPropertyName("guildId")]
        public ulong GuildId { get; set; }
    }
}