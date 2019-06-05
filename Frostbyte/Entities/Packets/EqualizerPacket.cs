using Frostbyte.Entities.Enums;

namespace Frostbyte.Entities.Packets
{
    public sealed class EqualizerPacket : PlayerPacket
    {
        public EqualizerPacket(ulong guildId) : base(guildId, OperationType.Equalizer)
        {
        }
    }
}