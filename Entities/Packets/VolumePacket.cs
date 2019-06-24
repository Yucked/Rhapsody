using Frostbyte.Entities.Enums;
using System.Text.Json.Serialization;

namespace Frostbyte.Entities.Packets
{
    public sealed class VolumePacket : PlayerPacket
    {
        public int Value { get; set; }

        public VolumePacket(ulong guildId) : base(guildId, OperationType.Volume)
        {
        }
    }
}