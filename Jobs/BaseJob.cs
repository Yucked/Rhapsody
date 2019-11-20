using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Concept.Jobs
{
    public abstract class BaseJob : BackgroundService
    {
        protected ILogger Logger { get; }
        protected TimeSpan Delay { get; protected set; }

        protected BaseJob(ILogger logger)
        {
            Logger = logger;
        }

        public void ChangeDelay(TimeSpan delay)
        {
            Delay = delay;
        }
    }
}