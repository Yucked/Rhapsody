using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using Frostbyte.Entities.Audio;

namespace Frostbyte.Entities.Results
{
    public class YouTubeResult
    {
        [JsonPropertyName("encrypted_id")]
        public string Id { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("author")]
        public string Author { get; set; }
    }

    public sealed class YouTubePlaylist : YouTubeResult
    {
        [JsonPropertyName("video")]
        public IEnumerable<YouTubeVideo> Videos { get; set; }

        public AudioPlaylist BuildPlaylist(string id, string url)
        {
            return new AudioPlaylist
            {
                Id = id,
                Url = url,
                Name = Title,
                Duration = Videos.Sum(x => x.Duration * 1000)
            };
        }
    }

    public sealed class YouTubeSearch
    {
        [JsonPropertyName("video")]
        public IEnumerable<YouTubeVideo> Video { get; set; }
    }

    public sealed class YouTubeVideo : YouTubeResult
    {
        [JsonPropertyName("length_seconds")]
        public long Duration { get; set; }

        [JsonPropertyName("user_id")]
        public string UserId { get; set; } // Figure out what ID this is

        [JsonIgnore]
        public AudioTrack ToTrack
            => new AudioTrack
            {
                Id = Id,
                Author = new TrackAuthor
                {
                    Name = Author
                },
                Title = Title,
                Duration = Duration * 1000,
                Provider = "YouTube",
                Url = $"https://www.youtube.com/watch?v={Id}",
                ArtworkUrl = $"https://img.youtube.com/vi/{Id}/maxresdefault.jpg"
            };
    }
}