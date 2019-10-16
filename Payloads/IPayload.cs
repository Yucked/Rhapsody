using System.Text.Json.Serialization;

namespace Concept.Payloads
{
    /// <summary>
    /// Represnts a generic payload
    /// </summary>
    public interface IPayload
    {
        [JsonPropertyName("op")]
        public PayloadOp Op { get; set; }
    }
}
