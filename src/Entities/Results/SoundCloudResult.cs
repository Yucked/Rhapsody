using System.Collections.Generic;
using System.Text.Json.Serialization;
using Frostbyte.Entities.Infos;

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
        public AuthorInfo ToAuthorInfo
            => new AuthorInfo(Username, PermalinkUrl, AvatarUrl);
    }

    public sealed class SoundCloudPlaylist : SoundCloudResult
    {
        [JsonPropertyName("tracks")]
        public IList<SoundCloudTrack> Tracks { get; set; }

        [JsonIgnore]
        public PlaylistInfo ToPlaylistInfo
            => new PlaylistInfo($"{Id}", Title, PermalinkUrl, Duration, ArtworkUrl);
    }

    public sealed class SoundCloudTrack : SoundCloudResult
    {
        [JsonIgnore]
        public TrackInfo ToTrackInfo
            => new TrackInfo($"{Id}", Title, PermalinkUrl, Duration, ArtworkUrl, IsStreamable, "SoundCloud",
                User.ToAuthorInfo);
    }

    public sealed class SoundCloudDirectUrl
    {
        [JsonPropertyName("http_mp3_128_url")]
        public string Url { get; set; }
    }
}