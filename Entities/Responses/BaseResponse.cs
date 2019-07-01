using Frostbyte.Entities.Enums;

namespace Frostbyte.Entities.Responses
{
    public class BaseResponse
    {
        public OperationType OP { get; set; }
        public object Data { get; set; }
        public string Error { get; set; }
    }
}