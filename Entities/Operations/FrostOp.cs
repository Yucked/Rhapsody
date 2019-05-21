using System;
using System.Collections.Generic;
using System.Text;

namespace Frostbyte.Entities.Operations
{
    public sealed class FrostOp : BaseOp
    {
        public ulong GuildId { get; set; }

        public FrostOp(string opCode) : base(opCode)
        {
        }
    }
}
