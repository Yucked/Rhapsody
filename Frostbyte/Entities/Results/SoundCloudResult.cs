using System.Collections.Generic;
using System.Text.Json.Serialization;
using Frostbyte.Entities.Audio;

namespace Frostbyte.Entities.Results
{
    public class SoundCloudResult
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("duration")]
        public long Duration { get; set; }

        [JsonPropertyName("streamable")]
        public bool IsStreamable { get; set; }

        [JsonPropertyName("artwork_url")]
        public string ArtworkUrl { get; set; }

        [JsonPropertyName("permalink_url")]
        public string PermalinkUrl { get; set; }

        [JsonPropertyName("user")]
        public SoundCloudUser User { get; set; }
    }

    public sealed class SoundCloudUser
    {
        [JsonPropertyName("permalink_url")]
        public string PermalinkUrl { get; set; }

        [JsonPropertyName("username")]
        public string Username { get; set; }

        [JsonPropertyName("avatar_url")]
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
        [JsonPropertyName("tracks")]
        public IList<SoundCloudTrack> Tracks { get; set; }

        [JsonIgnore]
        public AudioPlaylist ToPlaylist
            => new AudioPlaylist
            {
                Name = Title,
                Url = PermalinkUrl,
                Duration = Duration,
                ArtworkUrl = ArtworkUrl
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
                Url = PermalinkUrl,
                Duration = Duration,
                Author = User.ToAuthor,
                ArtworkUrl = ArtworkUrl,
                CanStream = IsStreamable
            };
    }

    public sealed class SoundCloudDirectUrl
    {
        [JsonPropertyName("http_mp3_128_url")]
        public string Url { get; set; }
    }
}