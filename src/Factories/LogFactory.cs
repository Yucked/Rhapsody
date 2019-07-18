using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Colorful;
using Frostbyte.Entities.Enums;
using Frostbyte.Entities.Results;
using Console = Colorful.Console;

namespace Frostbyte.Factories
{
    public sealed class LogFactory
    {
        private static readonly object LogLock
            = new object();

        public static void PrintHeader()
        {
            const string header = 
                @"                ___________                          __   ___.              __           
                \_   _____/_______   ____    _______/  |_ \_ |__   ___.__._/  |_   ____  
                 |    __)  \_  __ \ /  _ \  /  ___/\   __\ | __ \ <   |  |\   __\_/ __ \ 
                 |     \    |  | \/(  <_> ) \___ \  |  |   | \_\ \ \___  | |  |  \  ___/ 
                 \___  /    |__|    \____/ /____  > |__|   |___  / / ____| |__|   \___  >
                     \/                         \/             \/  \/                 \/ ";

            Console.WriteLine(header, Color.SpringGreen);
        }

        public static async Task PrintRepositoryInformationAsync()
        {
            var httpFactory = Singleton.Of<HttpFactory>();
            var result = new GitHubResult();
            var getBytes = await httpFactory
                .GetBytesAsync("https://api.github.com/repos/Yucked/Frostbyte")
                .ConfigureAwait(false);
            result.Repo = JsonSerializer.Parse<GitHubRepo>(getBytes.Span);

            getBytes = await httpFactory
                .WithUrl("https://api.github.com/repos/Yucked/Frostbyte/commits")
                .GetBytesAsync()
                .ConfigureAwait(false);
            result.Commit = JsonSerializer.Parse<IEnumerable<GitHubCommit>>(getBytes.Span).FirstOrDefault();

            Console.WriteLineFormatted(
                $"    {{0}}: {result.Repo.OpenIssues} opened   |    {{1}}: {result.Repo.License.Name}    | {{2}}: {result.Commit?.Sha}",
                Color.White,
                new Formatter("Issues", Color.Plum),
                new Formatter("License", Color.Plum),
                new Formatter("SHA", Color.Plum));
        }

        public static void PrintSystemInformation()
        {
            var process = Process.GetCurrentProcess();
            Console.WriteLineFormatted(
                $"    {{0}}: {RuntimeInformation.FrameworkDescription}    |    {{1}}: {RuntimeInformation.OSDescription}\n" +
                "    {2}: {3} ID / Using {4} Threads / Started On {5}",
                Color.White,
                new Formatter("FX Info", Color.Crimson),
                new Formatter("OS Info", Color.Crimson),
                new Formatter("Process", Color.Crimson),
                new Formatter(process.Id, Color.GreenYellow),
                new Formatter(process.Threads.Count, Color.GreenYellow),
                new Formatter($"{process.StartTime:MMM d - hh:mm:ss tt}", Color.GreenYellow));
        }

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