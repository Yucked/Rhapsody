using Frostbyte.Attributes;
using Frostbyte.Entities;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json.Serialization;
using System.IO;

namespace Frostbyte.Handlers
{
    [Service(ServiceLifetime.Singleton)]
    public sealed class ConfigHandler
    {
        public static ConfigEntity Config { get; private set; }

        public ConfigHandler()
        {
            Logger = new LogHandler<ConfigHandler>();
        }

        private LogHandler<ConfigHandler> Logger { get; }

        public ConfigEntity ValidateConfiguration()
        {
            if (File.Exists("./Config.json"))
            {
                Logger.LogInformation("Loaded Configuration!");
                var read = File.ReadAllText("./Config.json");
                Config = JsonSerializer.Parse<ConfigEntity>(read);
                return Config;
            }

            Config = new ConfigEntity
            {
                Host = "127.0.0.1",
                Password = "foobar",
                Port = 8080,
                Sources = new SourcesEntity
                {
                    Soundcloud = true,
                    Twitch = false,
                    Vimeo = false,
                    YouTube = true
                }
            };

            var json = JsonSerializer.ToBytes(Config);
            File.WriteAllBytes("./Config.json", json);

            Logger.LogWarning("Built and using default configuration.");
            return Config;
        }
    }
}