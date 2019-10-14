using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Concept.Payloads
{
    /// <summary>
    /// Represnts a generic payload.
    /// </summary>
    public class Payload
    {
        [JsonProperty("op")]
        public string Op { get; set; }
    }
}
