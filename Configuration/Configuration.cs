using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Concept.Configuration
{
    public class Configuration
    {
        public string Host { get; set; } = "localhost";

        public short Port { get; set; } = 5000;

        public string Authorization { get; set; } = "MyInvenciblePassword";

        public string LogLevel { get; set; } = "Information";

        public ConfigurationSources Sources { get; set; } = new ConfigurationSources();
    }
}