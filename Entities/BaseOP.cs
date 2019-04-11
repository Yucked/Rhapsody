using Newtonsoft.Json;

namespace Frostbyte.Entities
{
    public abstract class BaseOP
    {
        [JsonProperty("op")]
        private string OPCode { get; set; }

        protected BaseOP(string opCode)
        {
            OPCode = opCode;
        }
    }
}