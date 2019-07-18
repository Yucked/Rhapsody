using System.Text.Json.Serialization;
using Frostbyte.Entities.Enums;

namespace Frostbyte.Entities.Discord
{
    public struct HelloPayload
    {
        [JsonPropertyName("op")]
        public VoiceOpType Op { get; set; }

        [JsonPropertyName("d")]
        public HelloData Data { get; set; }
    }

    public struct HelloData
    {
        [JsonPropertyName("heartbeat_interval")]
        private int HbInterval { get; set; }

        public int Interval
            => (int) (HbInterval * 0.75f);
    }
}