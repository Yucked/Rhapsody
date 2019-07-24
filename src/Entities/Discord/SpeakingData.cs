using System.Text.Json.Serialization;

namespace Frostbyte.Entities.Discord
{
    public struct SpeakingData
    {
        [JsonPropertyName("speaking")]
        public bool IsSpeaking { get; set; }

        [JsonPropertyName("delay")]
        public int Delay { get; set; }

        [JsonPropertyName("ssrc")]
        public uint Ssrc { get; set; }
    }
}