using System.Text.Json.Serialization;

namespace Frostbyte.Entities
{
    public sealed class ResponseEntity
    {
        [JsonPropertyName("is")]
        public bool IsSuccess { get; set; }

        [JsonPropertyName("r")]
        public string Reason { get; set; }

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