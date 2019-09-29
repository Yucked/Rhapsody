using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Concept.Configuration
{
    public readonly struct ConfigurationLoader
    {
        private static Configuration _configurationCache = null;

        public readonly Configuration GetConfiguration()
        {
            if (CacheValid())
                return _configurationCache;

            if (Directory.Exists("config.json"))
                return CreateDefaultConfiguration();

            using var sr = new StreamReader($"config.json", Encoding.GetEncoding("iso-8859-1"));

            var json = sr.ReadToEnd();

            var config = JsonSerializer.Deserialize<Configuration>(json);

            PopuleCache(config);
            return config;
        }

        private readonly Configuration CreateDefaultConfiguration()
        {
            var config = new Configuration("localhost", 5000, "MyInvenciblePassword");

            var json = JsonSerializer.Serialize(config);

            File.AppendAllText("config.json", json);

            PopuleCache(config);
            return config;
        }

        private readonly void PopuleCache(Configuration config)
        {
            _configurationCache = config;
        }

        private bool CacheValid()
            => _configurationCache != null;
    }
}