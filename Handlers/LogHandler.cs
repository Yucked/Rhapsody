using Frostbyte.Extensions;
using System;
using System.Drawing;
using System.IO;
using Frostbyte.Entities.Enums;
using Console = Colorful.Console;

namespace Frostbyte.Handlers
{
    public sealed class LogHandler<T>
    {
            public static LogHandler<T> Log => LazyHelper.Value;
    
            private readonly DateTimeOffset _date;
            private readonly object _lockObj;
            private static readonly Lazy<LogHandler<T>> LazyHelper 
                = new Lazy<LogHandler<T>>(() => new LogHandler<T>());
            
        private LogHandler()
        {
            _lockObj = new object();
            _date = DateTimeOffset.Now;
        }

        public void Information(string message, Exception exception = default)
        {
            RawLog(LogLevel.Information, message, exception);
        }

        public void Debug(string message, Exception exception = default)
        {
            RawLog(LogLevel.Debug, message, exception);
        }

        public void Warning(string message, Exception exception = default)
        {
            RawLog(LogLevel.Warning, message, exception);
        }

        public void Error(Exception exception)
        {
            RawLog(LogLevel.Error, string.Empty, exception);
        }

        private void RawLog(LogLevel logLevel, string message, Exception exception)
        {                
            var date = $"[{DateTimeOffset.Now:MMM d - hh:mm:ss tt}]";
            var log = $" [{GetLogLevel(logLevel)}] ";
            var formatted = message.LogFormatter(exception);
            
            if (ConfigHandler.Config.LogLevel == logLevel && ConfigHandler.Config.LogLevel != LogLevel.None)
            {
                Append(date, Color.Gray);
                Append(log, GetColor(logLevel));
                Append(message, Color.White);
                Console.Write(Environment.NewLine);
            }

            var logMessage = $"{date}{log}[{typeof(T).Name}] {formatted}";
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
                File.AppendAllText($"{_date.Year}-{_date.Month}-{_date.Day}.log", message);
            }
        }
    }
}