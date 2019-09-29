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
            var substring = GetCategorySubstring(categoryName);

            if (_loggers.TryGetValue(substring, out var logger))
                return logger;

            var category = _configuration.GetSection(substring);
            logger = new ModifiedLogger(categoryName, category);
            _loggers.TryAdd(substring, logger);
            return logger;
        }

        public void Dispose()
            => _loggers.Clear();

        private string GetCategorySubstring(string categoryName)
            => categoryName switch
            {
                _ when categoryName.Contains("Microsoft") => "Microsoft",
                _ when categoryName.Contains("System")    => "System",
                _ when categoryName.Contains("Concept")   => "Concept",
                _                                         => "Default"
            };
    }
}