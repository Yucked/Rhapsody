using System.Text.Json.Serialization;

namespace Frostbyte.Entities.Discord
{
    public struct VoiceReadyPayload
    {
        [JsonPropertyName("ssrc")]
        public uint Ssrc { get; set; }

        [JsonPropertyName("ip")]
        public string IpAddress { get; set; }

        [JsonPropertyName("port")]
        public int Port { get; set; }

        [JsonPropertyName("modes")]
        public string[] Modes { get; set; }

        [JsonPropertyName("heartbeat_interval")]
        public int HeartbeatInterval { get; set; }
    }
}