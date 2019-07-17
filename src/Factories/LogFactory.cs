using System;
using System.Drawing;
using System.IO;
using Frostbyte.Entities.Enums;
using Console = Colorful.Console;

namespace Frostbyte.Factories
{
    public sealed class LogFactory
    {
        private static readonly object LogLock
            = new object();

        public static void Information<T>(string message)
        {
            RawLog<T>(LogType.Information, message, default);
        }

        public static void Debug<T>(string message)
        {
            RawLog<T>(LogType.Debug, message, default);
        }

        public static void Warning<T>(string message)
        {
            RawLog<T>(LogType.Warning, message, default);
        }

        public static void Error<T>(string message = default, Exception exception = default)
        {
            RawLog<T>(LogType.Error, message, exception);
        }

        private static void RawLog<T>(LogType logLevel, string message, Exception exception)
        {
            lock (LogLock)
            {
                var date = $"[{DateTimeOffset.Now:MMM d - hh:mm:ss tt}]";
                var log = $" [{GetLogLevel(logLevel)}] ";
                var msg = exception?.ToString() ?? message;

                if (string.IsNullOrWhiteSpace(msg))
                    return;

                Append(date, Color.Gray);
                Append(log, GetColor(logLevel));
                Append(msg, Color.White);
                Console.Write(Environment.NewLine);


                var logMessage = $"{date}{log}[{typeof(T).Name}] {msg}";
                File.AppendAllText("frostlog.log", $"{logMessage}\n");
            }
        }

        private static void Append(string message, Color color)
        {
            Console.ForegroundColor = color;
            Console.Write(message);
        }

        private static string GetLogLevel(LogType logLevel)
        {
            return logLevel switch
            {
                LogType.Debug       => "DBUG",
                LogType.Error       => "EROR",
                LogType.Information => "INFO",
                LogType.Warning     => "WARN",
                _                   => "NONE"
            };
        }

        private static Color GetColor(LogType logLevel)
        {
            return logLevel switch
            {
                LogType.Debug       => Color.SlateBlue,
                LogType.Error       => Color.Red,
                LogType.Information => Color.SpringGreen,
                LogType.Warning     => Color.Yellow,
                _                   => Color.SlateBlue
            };
        }
    }
}