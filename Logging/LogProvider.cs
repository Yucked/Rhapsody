using Microsoft.Extensions.Logging;

namespace Frostbyte.Logging
{
    public sealed class LogProvider : ILoggerProvider
    {
        public ILogger CreateLogger(string categoryName)
        {
            return new CustomLogger(categoryName);
        }

        public void Dispose()
        {
        }
    }
}