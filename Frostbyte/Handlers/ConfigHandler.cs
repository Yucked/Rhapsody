using Frostbyte.Attributes;
using Frostbyte.Entities;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json.Serialization;
using System.IO;
using Frostbyte.Entities.Enums;

namespace Frostbyte.Handlers
{
    [Service(ServiceLifetime.Singleton)]
    public sealed class ConfigHandler
    {
        public static ConfigEntity Config { get; private set; }

        public ConfigEntity ValidateConfiguration()
        {
            if (File.Exists("./Config.json"))
            {
                var read = File.ReadAllText("./Config.json");
                Config = JsonSerializer.Parse<ConfigEntity>(read);
                LogHandler<ConfigHandler>.Log.Information("Loaded Configuration!");
            }
            else
            {
                Config = new ConfigEntity
                {
                    Host = "127.0.0.1",
                    Password = "foobar",
                    Port = 8080,
                    LogLevel = LogLevel.Information,
                    Sources = new SourcesEntity
                    {
                        Soundcloud = true,
                        Twitch = false,
                        Vimeo = false,
                        YouTube = true,
                        Local = false
                    }
                };

                var json = JsonSerializer.ToUtf8Bytes(Config);
                File.WriteAllBytes("./Config.json", json);
                LogHandler<ConfigHandler>.Log.Warning("Built default configuration.");
            }

            return Config;
        }
    }
}