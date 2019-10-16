using System.Text.Json.Serialization;

namespace Concept.Payloads.InboundPayloads
{
    /// <summary>
    /// Represents a payload for connecting to the voice server.
    /// </summary>
    public sealed class ConnectPayload : InboundPayload
    {
        [JsonPropertyName("sessionId")]
        public string SessionId { get; set; }

        [JsonPropertyName("token")]
        public string Token { get; set; }

        [JsonPropertyName("endpoint")]
        public string Endpoint { get; set; }
    }
}
