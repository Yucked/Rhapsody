using Frostbyte.Entities.Enums;
using System.Text.Json.Serialization;

namespace Frostbyte.Entities.Packets
{
    public sealed class PausePacket : PlayerPacket
    {
        [JsonPropertyName("ip")]
        public bool IsPaused { get; set; }

        public PausePacket(ulong guildId) : base(guildId, OperationType.Pause)
        {
        }
    }
}