using System.Text.Json.Serialization;

namespace Frostbyte.Entities.Discord
{
    public struct BasePayload
    {
        [JsonPropertyName("op")]
        public int OpCode { get; }

        [JsonPropertyName("d")]
        public object Data { get; }

        public BasePayload(int opCode, object data)
        {
            OpCode = opCode;
            Data = data;
        }
    }
}