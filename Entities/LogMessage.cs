using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Concept.Entities
{
    public struct LogMessage
    {
        public LogLevel Level { get; set; }
        public string CategoryName { get; set; }
        public string Message { get; set; }

        public static string MessageFormat { get; } = "[{0}] [{1}] [{2}]\n    {3}";
    }
}
