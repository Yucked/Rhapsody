using System.Text.Json.Serialization;

namespace Frostbyte.Entities.Discord
{
    public struct BaseDiscordPayload
    {
        [JsonPropertyName("op")]
        public int OpCode { get; }

        [JsonPropertyName("d")]
        public object Data { get; }

        public BaseDiscordPayload(int opCode, object data)
        {
            OpCode = opCode;
            Data = data;
        }
    }
}