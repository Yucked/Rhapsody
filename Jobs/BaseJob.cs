using System;
using System.Threading;
using System.Threading.Tasks;

namespace Concept.Jobs
{
    public abstract class BaseJob
    {
        public abstract string Name { get; }
        protected TimeSpan Delay { get; private set; }
        protected CancellationTokenSource TokenSource { get; private set; }

        private Task _runningTask;

        public void Start()
        {
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