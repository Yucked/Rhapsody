using System.Text.Json.Serialization;

namespace Concept.Entities.Options
{
    public struct CacheOptions
    {
        [JsonPropertyName("limit")]
        public int Limit { get; set; }

        [JsonPropertyName("expiryInMinutes")]
        public long ExpiresAfter { get; set; }

        [JsonPropertyName("purgeDelay")]
        public int PurgeDelayMs { get; set; }

        [JsonPropertyName("metricsDelay")]
        public int MetricsDelayMs { get; set; }
    }
}