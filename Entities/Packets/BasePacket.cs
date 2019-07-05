using System.Text.Json.Serialization;
using Frostbyte.Entities.Enums;

namespace Frostbyte.Entities.Packets
{
    public abstract class BasePacket
    {
        protected BasePacket(OperationType operation)
        {
            OperationType = operation;
        }

        [JsonPropertyName("op")]
        public OperationType OperationType { get; set; }
    }
}