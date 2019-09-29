using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Concept.Loggers
{
    public class ConceptLoggerProvider : ILoggerProvider
    {
        private readonly ConcurrentDictionary<string, ConceptLogger> _loggers = new ConcurrentDictionary<string, ConceptLogger>();
        private readonly LogLevel MinimumLogLevel;

        public ConceptLoggerProvider(LogType minimumLogType)
            => MinimumLogLevel = ConvertLogType(minimumLogType);

        public ILogger CreateLogger(string categoryName)
            => _loggers.GetOrAdd(categoryName, name => new ConceptLogger(MinimumLogLevel, name));

        public void Dispose()
        {
            _loggers.Clear();
        }

        private LogLevel ConvertLogType(LogType logType)
            => logType switch
            {
                LogType.Debug => LogLevel.Trace,
                LogType.Verbose => LogLevel.Debug,
                LogType.Information => LogLevel.Information,
                LogType.Warning => LogLevel.Warning,
                LogType.Critical => LogLevel.Critical,
                _ => LogLevel.None
            };
    }
}