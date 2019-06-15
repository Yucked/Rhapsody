using System.Text.Json.Serialization;
using Frostbyte.Entities.Enums;

namespace Frostbyte.Entities.Packets
{
    public abstract class BasePacket
    {
        [JsonPropertyName("op")]
        public OperationType OperationType { get; set; }

        protected BasePacket(OperationType operation)
        {
            OperationType = operation;
        }
    }
}