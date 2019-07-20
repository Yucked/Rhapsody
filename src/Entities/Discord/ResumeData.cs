using System.Text.Json.Serialization;

namespace Frostbyte.Entities.Discord
{
    public struct ResumeData
    {
        [JsonPropertyName("server_id")]
        public string ServerId { get; set; }

        [JsonPropertyName("session_id")]
        public string SessionId { get; set; }

        [JsonPropertyName("token")]
        public string Token { get; set; }
    }
}