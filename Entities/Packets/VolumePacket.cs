using Frostbyte.Entities.Enums;

namespace Frostbyte.Entities.Packets
{
    public sealed class VolumePacket : PlayerPacket
    {
        public VolumePacket(ulong guildId) : base(guildId, OperationType.Volume)
        {
        }

        public int Value { get; set; }
    }
}