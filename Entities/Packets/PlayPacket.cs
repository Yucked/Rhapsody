using Frostbyte.Enums;

namespace Frostbyte.Entities.Packets
{
    public sealed class PlayPacket : PlayerPacket
    {
        public PlayPacket(ulong guildId) : base(guildId, Operation.Play)
        {
            
        }
    }
}