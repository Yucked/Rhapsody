using Frostbyte.Attributes;
using Frostbyte.Entities;
using Frostbyte.Handlers;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System.IO;

namespace Frostbyte.Factories
{
    [Service(ServiceLifetime.Singleton)]
    public sealed class ConfigFactory
    {
        private LogHandler<ConfigFactory> Logger { get; }

        public ConfigFactory()
        {
            Logger = new LogHandler<ConfigFactory>();
        }

        public ConfigEntity ValidateConfiguration()
        {
            if (File.Exists("./Config.json"))
            {
                Logger.LogInformation("Loaded Configuration!");
                var read = File.ReadAllText("./Config.json");
                return JsonConvert.DeserializeObject<ConfigEntity>(read);
            }

            var config = new ConfigEntity
            {
                Host = "127.0.0.1",
                Password = "foobar",
                Port = 6666,
                Sources = new SourcesEntity
                {
                    Soundcloud = true,
                    Twitch = false,
                    Vimeo = false,
                    YouTube = true
                }
            };

            var json = JsonConvert.SerializeObject(config);
            File.WriteAllText("./Config.json", json);

            Logger.LogWarning("Built and using default configuration.");
            return config;
        }
    }
}