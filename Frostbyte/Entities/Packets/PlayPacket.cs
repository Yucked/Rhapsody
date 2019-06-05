using System.Text.Json.Serialization;
using Frostbyte.Entities.Enums;

namespace Frostbyte.Entities.Packets
{
    public sealed class PlayPacket : PlayerPacket
    {
        [JsonPropertyName("q")]
        public string Query { get; set; }

        [JsonPropertyName("sTime")]
        public int StartTime { get; set; }

        [JsonPropertyName("eTime")]
        public int EndTime { get; set; }

        [JsonPropertyName("sr")]
        public bool ShouldReplace { get; set; }

        public PlayPacket(ulong guildId) : base(guildId, OperationType.Play)
        {
            
        }
    }
}