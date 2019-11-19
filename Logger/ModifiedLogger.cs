using System;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using Colorful;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Console = Colorful.Console;

namespace Concept.Logger
{
    public sealed class ModifiedLogger : ILogger
    {
        private readonly string _categoryName;
        private readonly IConfigurationSection _section;

        public ModifiedLogger(string categoryName, IConfigurationSection section)
        {
            _categoryName = categoryName;
            _section = section;
        }

        public IDisposable BeginScope<TState>(TState state)
            => default;

        public bool IsEnabled(LogLevel logLevel)
        {
            // TODO: Check logging properly.
            var level = (LogLevel)Enum.Parse(typeof(LogLevel), _section.Value);
            return level <= logLevel;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception,
            Func<TState, Exception, string> formatter)
        {
            var message = string.Empty;
            try
            {
                //IFeatureCollection disposing here, but with that try catch the log continues normal
                message = formatter(state, exception);
                if (string.IsNullOrWhiteSpace(message))
                    return;
            }
            catch
            {
                return;
            }

            LogWriter.WriteLog.Invoke(message, _categoryName, logLevel);
        }
    }
}