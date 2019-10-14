using System;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
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
                
                //IFeatureCollection is disposed here for some reason after disconnecting and reconnecting the client, and then sending a message
                var message = formatter(state, exception);
                if (string.IsNullOrWhiteSpace(message))
                    return;

                var date = DateTimeOffset.Now;
                var (color, abbrevation) = LogLevelInfo(logLevel);

                Append($"[{date:MMM d - hh:mm:ss tt}] ", Color.Gray);
                Append($"[{abbrevation}] ", color);
                Append($"[{_categoryName}]", Color.Orchid);
                Append($"{Environment.NewLine}  -> {message}", Color.White);
                Console.Write(Environment.NewLine);

                _semaphore.Release();
            });


        private void Append(string message, Color color)
        {
            Console.ForegroundColor = color;
            Console.Write(message);
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