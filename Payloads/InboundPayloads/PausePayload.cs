using Newtonsoft.Json;

namespace Concept.Payloads.InboundPayloads
{
    /// <summary>
    /// Represents a payload to pause a player.
    /// </summary>
    public class PausePayload : InboundPayload
    {
        [JsonProperty("pause")]
        public bool Pause { get; set; }
    }
}
