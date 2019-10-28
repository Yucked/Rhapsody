using System.Text.Json.Serialization;

namespace Concept.Entities.Options
{
    public struct CacheOptions
    {
        [JsonPropertyName("isEnabled")]
        public bool IsEnabled { get; set; }

        [JsonPropertyName("limit")]
        public int Limit { get; set; }

        [JsonPropertyName("expiryInMinutes")]
        public long ExpiresAfter { get; set; }
    }
}