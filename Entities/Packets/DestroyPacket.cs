using Frostbyte.Entities.Enums;

namespace Frostbyte.Entities.Packets
{
    public sealed class DestroyPacket : PlayerPacket
    {
        public DestroyPacket(ulong guildId) : base(guildId, OperationType.Destroy)
        {

        }
    }
}