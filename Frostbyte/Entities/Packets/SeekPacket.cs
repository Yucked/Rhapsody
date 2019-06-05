using Frostbyte.Entities.Enums;
using System.Text.Json.Serialization;

namespace Frostbyte.Entities.Packets
{
    public sealed class SeekPacket : PlayerPacket
    {
        [JsonPropertyName("pos")]
        public long Position { get; set; }

        public SeekPacket(ulong guildId) : base(guildId, OperationType.Seek)
        {
        }
    }
}