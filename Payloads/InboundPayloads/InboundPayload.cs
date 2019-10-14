using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Concept.Payloads.InboundPayloads
{
    /// <summary>
    /// Represents a payload sent by the client. This may also be sent as the stop or destroy payload.
    /// </summary>
    public class InboundPayload : Payload
    {
        [JsonProperty("guildId")]
        public string GuildId { get; set; }
    }
}
