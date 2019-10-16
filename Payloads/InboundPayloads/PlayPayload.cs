using System.Text.Json.Serialization;

namespace Concept.Payloads.InboundPayloads
{
    /// <summary>
    /// Represents a payload to play a track from a player.
    /// </summary>
    public sealed class PlayPayload : InboundPayload
    {
        [JsonPropertyName("track")]
        public string Track { get; set; }

#nullable enable
        [JsonPropertyName("startTime")]
        public string? StartTime { get; set; }

        [JsonPropertyName("endTime")]
        public string? EndTime { get; set; }
#nullable disable
        [JsonPropertyName("replace")]
        public bool Replace { get; set; }
    }
}
