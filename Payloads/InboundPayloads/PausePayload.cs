using System.Text.Json.Serialization;

namespace Concept.Payloads.InboundPayloads
{
    /// <summary>
    /// Represents a payload to pause a player.
    /// </summary>
    public sealed class PausePayload : InboundPayload
    {
        [JsonPropertyName("pause")]
        public bool Pause { get; set; }
    }
}
