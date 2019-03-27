using Frostbyte.Attributes;
using Frostbyte.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.IO;

namespace Frostbyte.Handlers
{
    [Service(ServiceLifetime.Singleton)]
    public sealed class ConfigHandler
    {
        private ILogger Logger { get; }

        public ConfigHandler(ILoggerFactory factory)
        {
            Logger = factory.CreateLogger<ConfigHandler>();
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