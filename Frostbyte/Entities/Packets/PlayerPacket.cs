using System.Text.Json.Serialization;
using Frostbyte.Entities.Enums;

namespace Frostbyte.Entities.Packets
{
    public abstract class PlayerPacket : BasePacket
    {
        [JsonPropertyName("g_id")]
        public ulong GuildId { get; }

        protected PlayerPacket(ulong guildId, Operation operation) : base(operation)
        {
            GuildId = guildId;
        }
    }
}