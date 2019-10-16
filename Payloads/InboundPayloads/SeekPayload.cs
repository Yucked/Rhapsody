using System.Text.Json.Serialization;

namespace Concept.Payloads.InboundPayloads
{
    /// <summary>
    /// Represnts a payload to seek through a playing track
    /// </summary>
    public sealed class SeekPayload : InboundPayload
    {
        [JsonPropertyName("position")]
        public int Position { get; set; }
    }
}
