using System;
using System.Text.Json.Serialization;

namespace Concept.Options
{
    public struct CacheOptions
    {
        [JsonPropertyName("isEnabled")]
        public bool IsEnabled { get; set; }

        [JsonPropertyName("limit")]
        public int Limit { get; set; }

        [JsonPropertyName("expiresAfter")]
        public TimeSpan ExpiresAfter { get; set; }
    }
}