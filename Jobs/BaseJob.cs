using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Concept.Jobs
{
    public abstract class BaseJob
    {
        protected ILogger Logger { get; }
        protected abstract string Name { get; }
        protected TimeSpan Delay { get; private set; }
        protected CancellationTokenSource TokenSource { get; private set; }

        private Task _runningTask;

        protected BaseJob(ILogger logger)
        {
            Logger = logger;
        }

        public void Start()
        {
            Logger.LogInformation($"Started {Name} job with {Delay.TotalSeconds}s delay.");
            Delay = TimeSpan.FromSeconds(5);
            TokenSource = new CancellationTokenSource();
            _runningTask = InitializeAsync();
        }

        protected abstract Task InitializeAsync();

        public void Stop()
        {
            TokenSource.Cancel(false);
            _runningTask?.Dispose();
        }

        public void ChangeDelay(TimeSpan delay)
        {
            Delay = delay;
        }
    }
}