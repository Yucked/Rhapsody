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
        private readonly SemaphoreSlim _semaphore;

        public ModifiedLogger(string categoryName, IConfigurationSection section)
        {
            _categoryName = categoryName;
            _section = section;
            _semaphore = new SemaphoreSlim(1, 1);
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
            => Task.Run(() =>
            {
                _semaphore.Wait();

                var message = formatter(state, exception);
                if (string.IsNullOrWhiteSpace(message))
                    return;

                var date = DateTimeOffset.Now;
                var (color, abbrevation) = logLevel.LogLevelInfo();

                const string logMessage = "[{0}] [{1}] [{2}]\n    {3}";
                var formatters = new[]
                {
                    new Formatter($"{date:MMM d - hh:mm:ss tt}", Color.Gray),
                    new Formatter(abbrevation, color),
                    new Formatter(_categoryName, color),
                    new Formatter(message, Color.Wheat)
                };

                Console.WriteLineFormatted(logMessage, Color.White, formatters);

                _semaphore.Release();
            });
    }
}