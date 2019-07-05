using Frostbyte.Entities.Enums;

namespace Frostbyte.Entities.Packets
{
    public sealed class ReadyPacket : PlayerPacket
    {
        public ReadyPacket(ulong guildId, OperationType operation) : base(guildId, operation)
        {
        }

        public bool ToggleCrossfade { get; set; }
    }
}