using System.Text.Json.Serialization;

namespace Frostbyte.Entities
{
    public sealed class MediaSources
    {
        [JsonPropertyName("use_yt")]
        public bool EnableYouTube { get; set; }

        [JsonPropertyName("use_sc")]
        public bool EnableSoundCloud { get; set; }

        [JsonPropertyName("use_twi")]
        public bool EnableTwitch { get; set; }

        [JsonPropertyName("use_vim")]
        public bool EnableVimeo { get; set; }

        [JsonPropertyName("use_lcl")]
        public bool EnableLocal { get; set; }
    }
}