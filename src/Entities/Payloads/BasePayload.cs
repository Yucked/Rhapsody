using Frostbyte.Entities.Enums;

namespace Frostbyte.Entities.Payloads
{
    public class BasePayload
    {
        public OperationType Op { get; set; }

        public ulong GuildId { get; set; }
    }
}