using System.Text.Json.Serialization;

namespace Concept.Payloads.InboundPayloads
{
    /// <summary>
    /// Reprents a payload to edit a player's volume
    /// </summary>
    public sealed class VolumePayload : InboundPayload
    {
        [JsonPropertyName("volume")]
        public int Volume { get; set; }
    }
}
