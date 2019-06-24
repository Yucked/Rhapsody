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
        public AudioSources Sources { get; set; }

        public int ReconnectInterval { get; set; }
        public int MaxConnectionRetries { get; set; }

        [JsonIgnore]
        internal string Url => $"http://{Host}:{Port}/";
    }
}

public sealed class AudioSources
{
    public bool EnableAppleMusic { get; set; }
    public bool EnableAudiomack { get; set; }
    public bool EnableBandCamp { get; set; }
    public bool EnableHttp { get; set; }
    public bool EnableLocal { get; set; }
    public bool EnableMixCloud { get; set; }
    public bool EnableMixer { get; set; }
    public bool EnableMusicBed { get; set; }
    public bool EnableSoundCloud { get; set; }
    public bool EnableTwitch { get; set; }
    public bool EnableVimeo { get; set; }
    public bool EnableYouTube { get; set; }
}