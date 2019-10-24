using System.Text.Json.Serialization;

namespace Concept.Entities
{
    public struct ApplicationMetric
    {
        [JsonPropertyName("server-uptime")]
        public long Uptime { get; set; }

        [JsonPropertyName("cpu-usage")]
        public double Usage { get; set; }

        [JsonPropertyName("connected-clients")]
        public int ConnectedClients { get; set; }

        [JsonPropertyName("connected-voice-clients")]
        public int ConnectedVoiceClients { get; set; }

        [JsonPropertyName("active-voice-clients")]
        public int ActiveVoiceClients { get; set; }
    }
}