using System.Text.Json.Serialization;
using Frostbyte.Entities.Enums;

namespace Frostbyte.Entities.Packets
{
    public sealed class VoiceUpdatePacket : PlayerPacket
    {
        public VoiceUpdatePacket(ulong guildId) : base(guildId, OperationType.VoiceUpdate)
        {
        }

        [JsonPropertyName("sess_id")]
        public string SessionId { get; set; }

        [JsonPropertyName("tkn")]
        public string Token { get; set; }

        [JsonPropertyName("ep")]
        public string EndPoint { get; set; }
    }
}