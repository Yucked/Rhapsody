using Newtonsoft.Json;

namespace Concept.Payloads.InboundPayloads
{
    /// <summary>
    /// Reprents a payload to edit a player's volume
    /// </summary>
    public class VolumePayload : InboundPayload
    {
        [JsonProperty("volume")]
        public int Volume { get; set; }
    }
}
