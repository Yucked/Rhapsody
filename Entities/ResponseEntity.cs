using Frostbyte.Entities.Enums;
using System.Text.Json.Serialization;

namespace Frostbyte.Entities
{
    public sealed class ResponseEntity
    {
        public bool IsSuccess { get; set; }

        public string Reason { get; set; }

        public OperationType Operation { get; set; }

        [JsonPropertyName("obj")]
        public object AdditionObject { get; set; }

        public ResponseEntity() { }

        public ResponseEntity(bool isSuccess, string reason)
        {
            IsSuccess = isSuccess;
            Reason = reason;
        }
    }
}