using Newtonsoft.Json;

namespace Concept.Payloads.InboundPayloads
{
    /// <summary>
    /// Represnts a payload to seek through a playing track
    /// </summary>
    public class SeekPayload : InboundPayload
    {
        [JsonProperty("position")]
        public int Position { get; set; }
    }
}
