using System.Text.Json.Serialization;

namespace Frostbyte.Entities.Results
{
    public sealed class SoundCloudTrack
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("permalink_url")]
        public string PermalinkUrl { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("duration")]
        public long Duration { get; set; }

        [JsonPropertyName("artwork_url")]
        public string ArtworkUrl { get; set; }

        [JsonPropertyName("original_format")]
        public string OriginalFormat { get; set; }

        [JsonIgnore]
        public TrackEntity ToTrack
            => new TrackEntity
            {
                Id = $"{Id}",
                Title = Title,
                ThumbnailUrl = ArtworkUrl,
                TrackLength = Duration,
                Url = PermalinkUrl
            };
    }

    public sealed class SoundCloudDirectUrl
    {
        [JsonPropertyName("http_mp3_128_url")]
        public string Url { get; set; }
    }
}