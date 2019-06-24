using Frostbyte.Entities.Enums;

namespace Frostbyte.Entities.Packets
{
    public sealed class ReadyPacket : PlayerPacket
    {
        public bool ToggleCrossfade { get; set; }

        public ReadyPacket(ulong guildId, OperationType operation) : base(guildId, operation)
        {
        }
    }
}