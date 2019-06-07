using System.Text.Json.Serialization;
using Frostbyte.Entities.Enums;

namespace Frostbyte.Entities
{
    public sealed class Configuration
    {
        public int Port { get; set; }
        public string Host { get; set; }
        public string Password { get; set; }
        public LogLevel LogLevel { get; set; }
        public MediaSources Sources { get; set; }

        [JsonIgnore]
        internal string Url
        {
            get { return $"http://{Host}:{Port}/"; }
        }
    }

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
