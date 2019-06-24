using Frostbyte.Entities.Enums;
using System.Text.Json.Serialization;

namespace Frostbyte.Entities.Discord
{
    public struct BaseDiscordPayload
    {
        [JsonPropertyName("op")]
        public VoiceOPType OP { get; }

        [JsonPropertyName("d")]
        public object Data { get; }

        public BaseDiscordPayload(VoiceOPType op, object data)
        {
            OP = op;
            Data = data;
        }
    }
}