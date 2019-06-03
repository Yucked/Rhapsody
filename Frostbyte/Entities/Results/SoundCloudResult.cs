using System.Text.Json.Serialization;
using Frostbyte.Entities.Audio;

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
        public Track ToTrack
            => new Track
            {
                Id = $"{Id}",
                Title = Title,
                ThumbnailUrl = ArtworkUrl,
                TrackLength = Duration,
                Url = PermalinkUrl
            };
    }
}