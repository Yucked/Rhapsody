using Frostbyte.Enums;
using System.Text.Json.Serialization;

namespace Frostbyte.Entities.Packets
{
    public abstract class PlayerPacket : BasePacket
    {
        [JsonPropertyName("g_id")]
        public ulong GuildId { get; set; }

        protected PlayerPacket(ulong guildId, Operation operation) : base(operation)
        {
            GuildId = guildId;
        }
    }
}