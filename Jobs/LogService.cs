using Colorful;
using Concept.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Concurrent;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;

using Console = Colorful.Console;

namespace Concept.Jobs
{
    public sealed class LogService : BackgroundService
    {
        internal ConcurrentQueue<LogMessage> _logQueue;
        public LogService(IConfiguration config)
        {
            _logQueue = new ConcurrentQueue<LogMessage>();
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while(!stoppingToken.IsCancellationRequested)
            {
                if (_logQueue.TryDequeue(out var msg))
                    await WriteLogAsync(msg);
                await Task.Delay(1);
              
            }
        }

        private Task WriteLogAsync(LogMessage message)
        {
            var date = DateTimeOffset.Now;
            var (color, abbrevation) = message.Level.LogLevelInfo();

            var formatters = new[]
            {
                    new Formatter($"{date:MMM d - hh:mm:ss tt}", Color.Gray),
                    new Formatter(abbrevation, color),
                    new Formatter(message.CategoryName, color),
                    new Formatter(message.Message, Color.Wheat)
            };

            Console.WriteLineFormatted(LogMessage.MessageFormat, Color.White, formatters);
            return Task.CompletedTask;
        }
    }
}
