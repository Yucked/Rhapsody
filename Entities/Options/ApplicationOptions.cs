using System;
using System.Text.Json.Serialization;

namespace Concept.Entities.Options
{
    public sealed class ApplicationOptions
    {
        [JsonPropertyName("hostname")]
        public string Hostname { get; set; }

        [JsonPropertyName("port")]
        public int Port { get; set; }

        [JsonPropertyName("authorization")]
        public string Authorization { get; set; }

        [JsonPropertyName("cacheOptions")]
        public CacheOptions CacheOptions { get; set; }


        public static ApplicationOptions Default
            => new ApplicationOptions
            {
                Hostname = "127.0.0.1",
                Port = 6969,
                Authorization = "Conceptual",
                CacheOptions = new CacheOptions
                {
                    IsEnabled = false,
                    Limit = 100,
                    ExpiresAfter = (long) TimeSpan.FromMinutes(30)
                        .TotalMinutes
                }
            };
    }
}