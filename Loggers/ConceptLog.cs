using Microsoft.Extensions.Logging;
using System;
using System.Drawing;
using System.IO;
using Console = Colorful.Console;

namespace Concept.Loggers
{
    public readonly struct ConceptLog
    {
        private static readonly object LockObj = new object();
        private static readonly string FileName = $"{nameof(Concept)}.log";

        public static void Information<T>(string message)
            => RawLog<T>(LogType.Information, message, default);

        public static void Debug<T>(string message)
            => RawLog<T>(LogType.Debug, message, default);

        public static void Warning<T>(string message)
            => RawLog<T>(LogType.Warning, message, default);

        public static void Error<T>(string message = default, Exception exception = default)
            => RawLog<T>(LogType.Error, message, exception);

        public static void RawLog(LogLevel logLevel, string message, string name, Exception exception)
        {
            lock (LockObj)
            {
                var (color, abbreviation) = GetLogTypeInfo(ConvertLog(logLevel));
                var date = $"[{DateTimeOffset.Now:MMM d - hh:mm:ss tt}]";
                var log = $" [{abbreviation}] ";
                var msg = exception?.ToString() ?? message;

                if (string.IsNullOrWhiteSpace(msg))
                    return;

                Append(date, Color.Gray);
                Append(log, color);
                Append(msg, Color.White);
                Console.Write(Environment.NewLine);

                var logMessage = $"{date}{log}[{name}] {msg}";
                File.AppendAllText(FileName, $"{logMessage}\n");
            }
        }

        private static void RawLog<T>(LogType logLevel, string message, Exception exception)
        {
            lock (LockObj)
            {
                var (color, abbreviation) = GetLogTypeInfo(logLevel);
                var date = $"[{DateTimeOffset.Now:MMM d - hh:mm:ss tt}]";
                var log = $" [{abbreviation}] ";
                var msg = exception?.ToString() ?? message;

                if (string.IsNullOrWhiteSpace(msg))
                    return;

                Append(date, Color.Gray);
                Append(log, color);
                Append(msg, Color.White);
                Console.Write(Environment.NewLine);

                var logMessage = $"{date}{log}[{typeof(T).Name}] {msg}";
                //File.AppendAllText(FileName, $"{logMessage}\n");
            }
        }

        private static (Color color, string abbreviation) GetLogTypeInfo(LogType logType)
            => logType switch
            {
                LogType.Debug => (Color.Orchid, "DBUG"),
                LogType.Verbose => (Color.Gray, "VEBS"),
                LogType.Information => (Color.LightGreen, "INFO"),
                LogType.Warning => (Color.Yellow, "WARN"),
                LogType.Error => (Color.DarkOrange, "EROR"),
                LogType.Critical => (Color.Crimson, "CRTC"),
                _ => (Color.DeepPink, "DFLT")
            };

        private static void Append(string message, Color color)
        {
            Console.ForegroundColor = color;
            Console.Write(message);
        }

        private static LogType ConvertLog(LogLevel logLevel)
            => logLevel switch
            {
                LogLevel.Trace => LogType.Debug,
                LogLevel.Debug => LogType.Verbose,
                LogLevel.Information => LogType.Information,
                LogLevel.Warning => LogType.Warning,
                LogLevel.Error => LogType.Error,
                LogLevel.None => LogType.Debug,
                _ => LogType.Debug
            };
    }
}