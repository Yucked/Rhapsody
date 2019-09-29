using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Concept.Loggers;

namespace Concept.Loggers
{
    public class ConceptLogger : ILogger
    {
        private readonly LogLevel MinimunLogLevel;
        private readonly string Name;

        public ConceptLogger(LogLevel minimumLogLevel, string name)
        {
            MinimunLogLevel = minimumLogLevel;
            Name = name;
        }

        public IDisposable BeginScope<TState>(TState state)
            => default;

        public bool IsEnabled(LogLevel logLevel)
            => logLevel >= MinimunLogLevel;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
                return;

            var message = formatter(state, exception);

            // Do it in a Task.Run to don't block the principal thread.
            _ = Task.Run(() => ConceptLog.RawLog(logLevel, message, Name, exception));
        }
    }
}