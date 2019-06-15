using System.Text.Json.Serialization;

namespace Frostbyte.Entities.Discord
{
    public struct IdentifyPayload
    {
        [JsonPropertyName("server_id")]
        public string ServerId { get; }

        [JsonPropertyName("user_id")]
        public string UserId { get; }

        [JsonPropertyName("session_id")]
        public string SessionId { get; }

        [JsonPropertyName("token")]
        public string Token { get; }

        public IdentifyPayload(ulong serverId, ulong userId, string sessionId, string token)
        {
            ServerId = $"{serverId}";
            UserId = $"{userId}";
            SessionId = sessionId;
            Token = token;
        }
    }
}