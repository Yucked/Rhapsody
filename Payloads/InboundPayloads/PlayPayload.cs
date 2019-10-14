using Newtonsoft.Json;

namespace Concept.Payloads.InboundPayloads
{
    /// <summary>
    /// Represents a payload to play a track from a player.
    /// </summary>
    public class PlayPayload : InboundPayload
    {
        [JsonProperty("track")]
        public string Track { get; set; }

#nullable enable
        [JsonProperty("startTime")]
        public string? StartTime { get; set; }

        [JsonProperty("endTime", NullValueHandling = NullValueHandling.Include)]
        public string? EndTime { get; set; }
#nullable disable
        [JsonProperty("replace", NullValueHandling = NullValueHandling.Include)]
        public bool Replace { get; set; }
    }
}
