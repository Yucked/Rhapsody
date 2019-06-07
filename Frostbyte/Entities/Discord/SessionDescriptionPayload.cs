using System.Text.Json.Serialization;

namespace Frostbyte.Entities.Discord
{
    public struct SessionDescriptionPayload
    {
        [JsonPropertyName("mode")]
        public string Mode { get; set; }

        [JsonPropertyName("secret_key")]
        public byte[] SecretKey { get; set; }
    }
}