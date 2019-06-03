using System.Collections.Generic;
using System.Text.Json.Serialization;
using Frostbyte.Entities.Audio;

namespace Frostbyte.Entities.Results
{
    public sealed class YouTubeResult
    {
        [JsonPropertyName("hits")]
        public long Hits { get; set; }

        [JsonPropertyName("video")]
        public IList<YouTubeVideo> Video { get; set; }
    }

    public sealed class YouTubeVideo
    {
        [JsonPropertyName("encrypted_id")]
        public string EncryptedId { get; set; }

        [JsonPropertyName("author")]
        public string Author { get; set; }

        [JsonPropertyName("likes")]
        public long Likes { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("length_seconds")]
        public long LengthSeconds { get; set; }

        [JsonPropertyName("views")]
        public string Views { get; set; }

        [JsonIgnore]
        public Track ToTrack
            => new Track
            {
                Id = EncryptedId,
                Author = new Author(Author),
                ThumbnailUrl = $"https://img.youtube.com/vi/{EncryptedId}/maxresdefault.jpg",
                Title = Title,
                TrackLength = LengthSeconds,
                Url = $"https://www.youtube.com/watch?v={EncryptedId}"
            };
    }
}