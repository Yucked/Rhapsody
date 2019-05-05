using Newtonsoft.Json;

namespace Frostbyte.Entities.Operations
{
    public abstract class BaseOp
    {
        protected BaseOp(string opCode)
        {
            OpCode = opCode;
        }

        [JsonProperty("op")]
        private string OpCode { get; set; }
    }
}