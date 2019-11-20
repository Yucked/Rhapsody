using System;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using Colorful;
using Concept.Entities;
using Concept.Jobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Console = Colorful.Console;

namespace Concept.Logger
{
    public sealed class ModifiedLogger : ILogger
    {
        private readonly LogService _service;
        private readonly string _categoryName;
        private readonly IConfigurationSection _section;

        public ModifiedLogger(string categoryName, IConfigurationSection section, LogService service)
        {
            _categoryName = categoryName;
            _section = section;
            _service = service;
        }

        public IDisposable BeginScope<TState>(TState state)
            => default;

        public bool IsEnabled(LogLevel logLevel)
        {
            // TODO: Check logging properly.
            var level = (LogLevel) Enum.Parse(typeof(LogLevel), _section.Value);
            return level <= logLevel;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception,
            Func<TState, Exception, string> formatter)
        {
            var message = formatter(state, exception);
            if (string.IsNullOrWhiteSpace(message))
                return;

            var logMessage = new LogMessage { CategoryName = _categoryName, Level = logLevel, Message = message };
            _service._logQueue.Enqueue(logMessage);
        }
    }
}