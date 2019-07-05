using System.Text.Json.Serialization;
using Frostbyte.Entities.Enums;

namespace Frostbyte.Entities.Discord
{
    public struct BaseDiscordPayload
    {
        [JsonPropertyName("op")]
        public VoiceOpType Op { get; }

        [JsonPropertyName("d")]
        public object Data { get; }

        public BaseDiscordPayload(VoiceOpType op, object data)
        {
            Op = op;
            Data = data;
        }
    }
}