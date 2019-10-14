using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Concept.Payloads.InboundPayloads
{
    /// <summary>
    /// Represents a payload for connecting to the voice server.
    /// </summary>
    public class ConnectPayload : InboundPayload
    {
        [JsonProperty("sessionId")]
        public string SessionId { get; set; }

        [JsonProperty("token")]
        public string Token { get; set; }

        [JsonProperty("endpoint")]
        public string Endpoint { get; set; }
    }
}
