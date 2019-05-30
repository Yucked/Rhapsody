using System.Text.Json.Serialization;

namespace Frostbyte.Entities.Discord
{
    public struct SelectPayload
    {
        [JsonPropertyName("protocol")]
        public string Protocol { get; }

        [JsonPropertyName("data")]
        public Data Data { get; }

        public SelectPayload(string address, int port)
        {
            Protocol = "udp";
            Data = new Data
            {
                Address = address,
                Port = port,
                Mode = "xsalsa20_poly1305"
            };
        }
    }

    public struct Data
    {
        [JsonPropertyName("address")]
        public string Address { get; set; }

        [JsonPropertyName("port")]
        public int Port { get; set; }

        [JsonPropertyName("mode")]
        public string Mode { get; set; }
    }
}