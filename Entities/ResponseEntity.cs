using System.Text.Json.Serialization;

namespace Frostbyte.Entities
{
    public sealed class ResponseEntity
    {
        [JsonPropertyName("is")]
        public bool IsSuccess { get; set; }

        [JsonPropertyName("r")]
        public string Reason { get; set; }
    }
}