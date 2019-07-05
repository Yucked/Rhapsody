using Frostbyte.Entities.Enums;

namespace Frostbyte.Entities.Packets
{
    public sealed class PausePacket : PlayerPacket
    {
        public PausePacket(ulong guildId) : base(guildId, OperationType.Pause)
        {
        }

        public bool IsPaused { get; set; }
    }
}