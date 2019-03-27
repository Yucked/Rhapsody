using System;
using System.Drawing;
using System.IO;
using Microsoft.Extensions.Logging;
using Console = Colorful.Console;

namespace Frostbyte.Logging
{
    public sealed class CustomLogger : ILogger
    {
        private readonly string _catName;
        private readonly object _lockObj;

        public CustomLogger(string catName)
        {
            _catName = catName;
            _lockObj = new object();
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return default;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            var message = formatter(state, exception);
            var date = $"[{DateTimeOffset.Now:MMM d - hh:mm: ss tt}]";
            var log = $" [{GetLogLevel(logLevel)}] ";

            Append(date, Color.Gray);
            Append(log, GetColor(logLevel));
            Append(message, Color.White);
            Console.Write(Environment.NewLine);

            var logMessage = $"{date}{log}[{_catName}] {message}";
            WriteToFile(logMessage);
        }

        private void Append(string message, Color color)
        {
            Console.ForegroundColor = color;
            Console.Write(message);
        }

        private string GetLogLevel(LogLevel logLevel)
        {
            return logLevel switch
            {
                LogLevel.Critical       => "CRIT",
                LogLevel.Debug          => "DBUG",
                LogLevel.Error          => "EROR",
                LogLevel.Information    => "INFO",
                LogLevel.None           => "NONE",
                LogLevel.Trace          => "TRCE",
                LogLevel.Warning        => "WARN",
                _                       => "NONE"
            };
        }

        private Color GetColor(LogLevel logLevel)
        {
            return logLevel switch
            {
                LogLevel.Critical       => Color.Red,
                LogLevel.Debug          => Color.SlateBlue,
                LogLevel.Error          => Color.Red,
                LogLevel.Information    => Color.SpringGreen,
                LogLevel.None           => Color.BurlyWood,
                LogLevel.Trace          => Color.SlateBlue,
                LogLevel.Warning        => Color.Yellow,
                _                       => Color.SlateBlue
            };
        }

        private void WriteToFile(string message)
        {
            lock (_lockObj)
            {
                var date = DateTime.Now;
                File.WriteAllLines($"{date.Year}-{date.Month}-{date.Year}.log", new[] { message});
            }
        }
    }
}