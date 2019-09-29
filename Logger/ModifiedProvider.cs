using System;
using System.Collections.Concurrent;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Concept.Logger
{
    public readonly struct ModifiedProvider : ILoggerProvider
    {
        private readonly IConfigurationSection _configuration;
        private readonly ConcurrentDictionary<string, ModifiedLogger> _loggers;

        public ModifiedProvider(IConfiguration configuration)
        {
            _configuration = configuration.GetSection("LogLevel");
            _loggers = new ConcurrentDictionary<string, ModifiedLogger>();
        }

        public ILogger CreateLogger(string categoryName)
        {
            categoryName = GetCategory(categoryName);

            if (_loggers.TryGetValue(categoryName, out var logger))
                return logger;

            var category = _configuration.GetSection(categoryName);
            logger = new ModifiedLogger(categoryName, category);
            _loggers.TryAdd(categoryName, logger);
            return logger;
        }

        public void Dispose()
            => _loggers.Clear();

        private string GetCategory(string categoryName)
            => categoryName switch
            {
                _ when categoryName.Contains("Microsoft") => "Microsoft",
                _ when categoryName.Contains("System") => "System",
                _ when categoryName.Contains("Theory") => "Theory",
                _ => "Default"
            };
    }
}