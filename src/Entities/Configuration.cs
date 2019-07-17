using Frostbyte.Entities.Enums;

namespace Frostbyte.Entities
{
    public struct Configuration
    {
        public ServerConfig Server { get; set; }
        public AudioConfig Audio { get; set; }
        public LogType LogType { get; set; }
    }

    public struct ServerConfig
    {
        public ushort Port { get; set; }
        public string Hostname { get; set; }
        public string Authorization { get; set; }
        public bool UseRandomPort { get; set; }
        public int ReconnectInterval { get; set; }
        public int MaxReconnectTries { get; set; }
        public ushort BufferSize { get; set; }
    }

    public struct AudioConfig
    {
        public OpusVoiceType OpusVoiceType { get; set; }
        public SourcesConfig Sources { get; set; }
    }

    public struct SourcesConfig
    {
        public bool AppleMusic { get; set; }
        public bool Audiomack { get; set; }
        public bool BandCamp { get; set; }
        public bool Http { get; set; }
        public bool Local { get; set; }
        public bool MixCloud { get; set; }
        public bool Mixer { get; set; }
        public bool MusicBed { get; set; }
        public bool SoundCloud { get; set; }
        public bool Twitch { get; set; }
        public bool Vimeo { get; set; }
        public bool YouTube { get; set; }
    }
}