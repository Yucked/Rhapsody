using System.Text.Json.Serialization;

namespace Concept.Payloads.InboundPayloads
{
    public class InboundPayload : IPayload
    {
        [JsonPropertyName("op")]
        public PayloadOp Op { get; set; }

        [JsonPropertyName("guildId")]
        public string GuildId { get; set; }
    }
}
