using System.Text.Json.Serialization;

namespace Test.Payloads.Inbound
{
    public sealed class ConnectPayload : BasePayload
    {
        [JsonPropertyName("sessionId")]
        public string SessionId { get; set; }

        [JsonPropertyName("token")]
        public string Token { get; set; }

        [JsonPropertyName("endpoint")]
        public string Endpoint { get; set; }
    }
}