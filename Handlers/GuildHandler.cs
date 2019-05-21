using Frostbyte.Entities.Operations;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Frostbyte.Handlers
{
    public sealed class GuildHandler
    {
        public Func<FrostOp, Task> OnMessage;
    }
}