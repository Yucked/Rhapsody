using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Concept.Loggers
{
    public static class ConceptLoggerExtensions
    {
        public static ILoggingBuilder AddConcept(this ILoggingBuilder loggerBuilder, LogType logType = LogType.Information)
            => loggerBuilder.AddProvider(new ConceptLoggerProvider(logType));
    }
}