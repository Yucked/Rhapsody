using System.Text.Json.Serialization;
using Frostbyte.Entities.Enums;

namespace Frostbyte.Entities.Packets
{
    public sealed class SeekPacket : PlayerPacket
    {
        public SeekPacket(ulong guildId) : base(guildId, OperationType.Seek)
        {
        }

        [JsonPropertyName("pos")]
        public long Position { get; set; }
    }
}