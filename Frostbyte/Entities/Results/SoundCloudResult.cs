using System.Collections.Generic;
using System.Text.Json.Serialization;
using Frostbyte.Entities.Audio;

namespace Frostbyte.Entities.Results
{
    public class SoundCloudResult
    {
        public long Id { get; set; }

        public string Title { get; set; }

        public long Duration { get; set; }

        [JsonPropertyName("artwork_url")]
        public string ArtworkUrl { get; set; }

        [JsonPropertyName("permalink_url")]
        public string PermalinkUrl { get; set; }

        public SoundCloudUser User { get; set; }
    }

    public sealed class SoundCloudUser
    {
        public string PermalinkUrl { get; set; }

        public string Username { get; set; }

        public string AvatarUrl { get; set; }

        [JsonIgnore]
        public TrackAuthor ToAuthor
            => new TrackAuthor
            {
                Name = Username,
                Url = PermalinkUrl,
                AvatarUrl = AvatarUrl
            };
    }

    public sealed class SoundCloudPlaylist : SoundCloudResult
    {
        public IList<SoundCloudTrack> Tracks { get; set; }

        [JsonIgnore]
        public AudioPlaylist ToPlaylist
            => new AudioPlaylist
            {
                Name = Title,
                Url = PermalinkUrl,
                Duration = Duration
            };
    }

    public sealed class SoundCloudTrack : SoundCloudResult
    {
        [JsonIgnore]
        public AudioTrack ToTrack
            => new AudioTrack
            {
                Id = $"{Id}",
                Title = Title,
                ArtworkUrl = ArtworkUrl,
                Duration = Duration,
                Url = PermalinkUrl
            };
    }

    public sealed class SoundCloudDirectUrl
    {
        [JsonPropertyName("http_mp3_128_url")]
        public string Url { get; set; }
    }
}