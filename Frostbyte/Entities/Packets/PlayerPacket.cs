using System.Text.Json.Serialization;
using Frostbyte.Entities.Enums;

namespace Frostbyte.Entities.Packets
{
    public abstract class PlayerPacket : BasePacket
    {
        [JsonPropertyName("g_id")]
        public ulong GuildId { get; }

        [JsonPropertyName("usr_id")]
        public ulong UserId { get; set; }

        protected PlayerPacket(ulong guildId, OperationType operation) : base(operation)
        {
            GuildId = guildId;
        }
    }
}