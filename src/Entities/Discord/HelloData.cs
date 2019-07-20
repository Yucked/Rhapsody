using System.Text.Json.Serialization;

namespace Frostbyte.Entities.Discord
{
    public struct HelloData
    {
        [JsonPropertyName("heartbeat_interval")]
        private int HbInterval { get; set; }

        public int Interval
            => (int) (HbInterval * 0.75f);
    }
}