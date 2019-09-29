using System;
using System.Drawing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Console = Colorful.Console;

namespace Concept.Logger
{
    public readonly struct ModifiedLogger : ILogger
    {
        private readonly object _lockObj;
        private readonly string _categoryName;
        private readonly IConfigurationSection _section;

        public ModifiedLogger(string categoryName, IConfigurationSection section)
        {
            _categoryName = categoryName;
            _section = section;
            _lockObj = new object();
        }

        public IDisposable BeginScope<TState>(TState state)
            => default;

        public bool IsEnabled(LogLevel logLevel)
        {
            var level = (LogLevel) Enum.Parse(typeof(LogLevel), _section.Value);
            return true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception,
            Func<TState, Exception, string> formatter)
        {
            lock (_lockObj)
            {
                var message = formatter(state, exception);
                if (string.IsNullOrWhiteSpace(message))
                    return;

                var date = $"[{DateTimeOffset.Now:MMM d - hh:mm:ss tt}] ";
                var (color, abbrevation) = LogLevelInfo(logLevel);

                Append(date, Color.Gray);
                Append($"[{abbrevation}] ", color);
                Append($"<{_categoryName}>", Color.Orchid);
                Append($"{Environment.NewLine}  -> {message}", Color.White);
                Console.Write(Environment.NewLine);
            }
        }


        private void Append(string message, Color color)
        {
            lock (_lockObj)
            {
                Console.ForegroundColor = color;
                Console.Write(message);
            }
        }

        private (Color Color, string Abbrevation) LogLevelInfo(LogLevel logLevel)
            => logLevel switch
            {
                LogLevel.Information => (Color.SpringGreen, "INFO"),
                LogLevel.Debug       => (Color.MediumPurple, "DBUG"),
                LogLevel.Trace       => (Color.MediumPurple, "TRCE"),
                LogLevel.Critical    => (Color.Crimson, "CRIT"),
                LogLevel.Error       => (Color.Crimson, "EROR"),
                LogLevel.Warning     => (Color.Orange, "WARN"),
                _                    => (Color.Tomato, "UKNW")
            };
    }
}