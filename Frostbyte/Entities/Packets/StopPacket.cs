using Frostbyte.Entities.Enums;

namespace Frostbyte.Entities.Packets
{
    public sealed class StopPacket : PlayerPacket
    {
        public StopPacket(ulong guildId) : base(guildId, OperationType.Stop)
        {
        }
    }
}