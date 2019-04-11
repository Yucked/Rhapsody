using Newtonsoft.Json;

namespace Frostbyte.Entities
{
    public sealed class ResponseEntity
    {
        [JsonProperty("is")]
        public bool IsSuccess { get; set; }

        [JsonProperty("r")]
        public string Reason { get; set; }
    }
}