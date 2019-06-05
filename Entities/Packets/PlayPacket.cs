using System.Text.Json.Serialization;
using Frostbyte.Entities.Enums;

namespace Frostbyte.Entities.Packets
{
    public sealed class PlayPacket : PlayerPacket
    {
        [JsonPropertyName("track")]
        public string Hash { get; set; }

        [JsonPropertyName("startTime")]
        public int StartTime { get; set; }

        [JsonPropertyName("endTime")]
        public int EndTime { get; set; }

        public PlayPacket(ulong guildId) : base(guildId, OperationType.Play)
        {
            
        }
    }
}