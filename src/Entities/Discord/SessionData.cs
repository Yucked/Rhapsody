using System.Text.Json.Serialization;

namespace Frostbyte.Entities.Discord
{
    public struct SessionData
    {
        [JsonPropertyName("mode")]
        public string Mode { get; set; }

        [JsonPropertyName("media_session_id")]
        public string MediaSessionId { get; set; }

        [JsonPropertyName("video_codec")]
        public string VideoCodec { get; set; }

        [JsonPropertyName("audio_codec")]
        public string AudioCodec { get; set; }

        [JsonPropertyName("secret_key")]
        public int[] Secret { get; set; }
    }
}