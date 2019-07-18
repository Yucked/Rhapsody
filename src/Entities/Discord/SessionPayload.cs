using System.Text.Json.Serialization;
using Frostbyte.Entities.Enums;

namespace Frostbyte.Entities.Discord
{
    public struct SessionPayload
    {
        [JsonPropertyName("op")]
        public VoiceOpType Op { get; set; }

        [JsonPropertyName("d")]
        public SessionData Data { get; set; }
    }

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