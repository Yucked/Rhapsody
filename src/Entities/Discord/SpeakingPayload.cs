using System.Text.Json.Serialization;

namespace Frostbyte.Entities.Discord
{
    public struct SpeakingPayload
    {
        [JsonPropertyName("speaking")]
        public bool IsSpeaking { get; set; }

        [JsonPropertyName("delay")]
        public int Delay { get; set; }

        [JsonPropertyName("ssrc")]
        public uint SSRC { get; set; }
    }
}