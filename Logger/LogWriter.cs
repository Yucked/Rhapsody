using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using Colorful;
using Microsoft.Extensions.Logging;
using Console = Colorful.Console;

namespace Concept.Logger
{
    // Not readonly because this struct has not readonly fields.
    public struct LogWriter
    {
        // An concrete readonly struct only for save better in the Queue<>.
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

        public LogWriter(int _)
        {
            Semaphore = new SemaphoreSlim(1, 1);
            LogsQueue = new Queue<LogMessage>();
            QueueStopped = true;
            WriteLog = null;
            OnLog += LogReceived;
        }

        // An void method for the Loggers don't need to make an async context.
        public Action<string, string, LogLevel> WriteLog;

        // An event thar indicates when a LogMessage is received.
        private event Action<string, string, LogLevel> OnLog
        {
            add => WriteLog += value;
            remove => WriteLog -= value;
        }

        private readonly SemaphoreSlim Semaphore;

        private readonly Queue<LogMessage> LogsQueue;
        private bool QueueStopped { get; set; }

        // If the Semaphore is clear and the Queue as stopped we can create a new thread.
        private bool CanCreateThread
            => QueueStopped && Semaphore.CurrentCount > 0;

        private void LogReceived(string message, string categoryName, LogLevel logLevel)
        {
            // Here we add the message to the logqueue.
            LogsQueue.Enqueue(new LogMessage(message, categoryName, logLevel));

            // Pass to that method.
            TryCreateNewThread();
        }

        // ValueTask perform better in this case, and if in the future we add
        // a file writer for the logs it will perform better in async context.
        private async ValueTask WriteLogAsync(LogMessage logQueueMessage)
        {
            // For the sake of safety we put a SemaphoreSlim here so that
            // really no problem if eventually another thread ends up here.
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

            // Release the SemaphoreSlim and try write the next log.
            Semaphore.Release();
            await WriteNextLogAsync();
        }

        private async ValueTask WriteNextLogAsync()
        {
            // Verify if have any log in queue if no we stop the queue.
            // If yes we dequeue the next LogMessage and write.
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

        private void TryCreateNewThread()
        {
            // If don't have one thread writing the logs we create one.
            // This Extension method is because in struct we can't do anoymous methods.
            if (CanCreateThread)
                Extensions.RunAsyncValueTask(WriteNextLogAsync);
        }
    }
}