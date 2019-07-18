using System.Text.Json.Serialization;
using Frostbyte.Entities.Enums;

namespace Frostbyte.Entities.Discord
{
    public struct ReadyPayload
    {
        [JsonPropertyName("op")]
        public VoiceOpType Op { get; set; }

        [JsonPropertyName("d")]
        public ReadyData Data { get; set; }
    }

    public struct ReadyData
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