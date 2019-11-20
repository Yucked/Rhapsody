using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Colorful;
using Microsoft.Extensions.Logging;
using Console = Colorful.Console;

namespace Concept.Jobs
{
    public sealed class LogJob : BaseJob
    {
        private readonly struct LogMessage
        {
            public LogMessage(string message, string categoryName, LogLevel logLevel)
            {
                Message = message;
                CategoryName = categoryName;
                LogLevel = logLevel;
            }

            public readonly string Message { get; }

            public readonly string CategoryName { get; }

            public readonly LogLevel LogLevel { get; }
        }

        public LogJob() : base(default)
        {
            OnLog += LogReceived;
        }

        public Action<string, string, LogLevel> WriteLog;

        private event Action<string, string, LogLevel> OnLog
        {
            add => WriteLog += value;
            remove => WriteLog -= value;
        }

        private readonly SemaphoreSlim Semaphore = new SemaphoreSlim(1, 1);

        private readonly Queue<LogMessage> LogsQueue = new Queue<LogMessage>();
        private bool QueueStopped = true;

        private bool CanCreateThred
            => QueueStopped && Semaphore.CurrentCount > 0;

        protected override string Name { get; } = "LogWriter";

        protected override async Task InitializeAsync()
        {
            if (CanCreateThred)
                await WriteNextLogAsync();
        }

        private void LogReceived(string message, string categoryName, LogLevel logLevel)
        {
            LogsQueue.Enqueue(new LogMessage(message, categoryName, logLevel));

            if (CanCreateThred)
                _ = Task.Run(async () => await WriteNextLogAsync());
        }

        private async Task WriteLogAsync(LogMessage logQueueMessage)
        {
            await Semaphore.WaitAsync();
            var date = DateTimeOffset.Now;
            var (color, abbrevation) = logQueueMessage.LogLevel.LogLevelInfo();

            const string logMessage = "[{0}] [{1}] [{2}]\n    {3}";
            var formatters = new[]
            {
                    new Formatter($"{date:MMM d - hh:mm:ss tt}", Color.Gray),
                    new Formatter(abbrevation, color),
                    new Formatter(logQueueMessage.CategoryName, color),
                    new Formatter(logQueueMessage.Message, Color.Wheat)
            };

            Console.WriteLineFormatted(logMessage, Color.White, formatters);

            Semaphore.Release();
            await WriteNextLogAsync();
        }

        private async Task WriteNextLogAsync()
        {
            if (LogsQueue.Count <= 0)
            {
                QueueStopped = true;
            }
            else
            {
                QueueStopped = false;
                await WriteLogAsync(LogsQueue.Dequeue());
            }
        }
    }
}