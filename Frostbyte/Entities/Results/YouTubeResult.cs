using System.Collections.Generic;
using System.Text.Json.Serialization;
using Frostbyte.Entities.Audio;

namespace Frostbyte.Entities.Results
{
    public sealed class YouTubeResult
    {
        [JsonPropertyName("video")]
        public IList<YouTubeVideo> Video { get; set; }
    }

    public sealed class YouTubeVideo
    {
        [JsonPropertyName("encrypted_id")]
        public string EncryptedId { get; set; }

        [JsonPropertyName("author")]
        public string Author { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("length_seconds")]
        public long LengthSeconds { get; set; }

        [JsonIgnore]
        public AudioTrack ToTrack
            => new AudioTrack
            {
                Id = EncryptedId,
                Author = new TrackAuthor
                {
                    Name = Author
                },
                ArtworkUrl = $"https://img.youtube.com/vi/{EncryptedId}/maxresdefault.jpg",
                Title = Title,
                Duration = LengthSeconds,
                Url = $"https://www.youtube.com/watch?v={EncryptedId}"
            };
    }
}