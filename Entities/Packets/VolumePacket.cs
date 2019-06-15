using Frostbyte.Entities.Enums;
using System.Text.Json.Serialization;

namespace Frostbyte.Entities.Packets
{
    public sealed class VolumePacket : PlayerPacket
    {
        [JsonPropertyName("vol")]
        public int Volume { get; set; }

        public VolumePacket(ulong guildId) : base(guildId, OperationType.Volume)
        {
        }
    }
}