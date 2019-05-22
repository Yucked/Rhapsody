using Frostbyte.Enums;
using System.Text.Json.Serialization;

namespace Frostbyte.Entities.Packets
{
    public abstract class BasePacket
    {
        [JsonPropertyName("op")]
        public Operation Operation { get; set; }

        protected BasePacket(Operation operation)
        {
            Operation = operation;
        }
    }
}