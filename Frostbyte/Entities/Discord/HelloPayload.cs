using System.Text.Json.Serialization;

namespace Frostbyte.Entities.Discord
{
    public struct HelloPayload
    {
        [JsonPropertyName("heartbeat_interval")]
        private int HbInterval { get; set; }

        public int HeartBeatInterval => (int) (HbInterval * 0.75f);
    }
}