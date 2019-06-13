using Frostbyte.Entities.Enums;

namespace Frostbyte.Entities.Packets
{
    public sealed class PlayPacket : PlayerPacket
    {
        public string Hash { get; set; }
        public int StartTime { get; set; }
        public int EndTime { get; set; }
        public bool ShouldReplace { get; set; }

        public PlayPacket(ulong guildId) : base(guildId, OperationType.Play)
        {

        }
    }
}