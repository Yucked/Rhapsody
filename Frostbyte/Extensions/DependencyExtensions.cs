using Frostbyte.Attributes;
using Frostbyte.Entities;
using Frostbyte.Entities.Enums;
using Frostbyte.Handlers;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;

namespace Frostbyte.Extensions
{
    public static class DependencyExtensions
    {
        /// <summary>
        /// Adds all classes that have <see cref="ServiceAttribute"/> declared.
        /// </summary>
        public static IServiceCollection AddAttributeServices(this IServiceCollection services)
        {
            var matches = Assembly.GetExecutingAssembly().GetTypes().Where(x => x.GetCustomAttribute<RegisterServiceAttribute>() != null).ToArray();

            if (matches.Length is 0)
                return services;

            foreach (var match in matches)
            {
                var attr = match.GetCustomAttribute<RegisterServiceAttribute>();
                if (attr.BaseType != null)
                {
                    services.AddSingleton(attr.BaseType, match);
                }
                else
                {
                    services.AddSingleton(match);
                }

                LogHandler<IServiceCollection>.Log.Debug($"Added {match.Name} as Singleton" +
                    (attr.BaseType != null ? $" with {attr.BaseType.Name} as ServiceType." : "."));
            }

            return services;
        }

        /// <summary>
        /// Adds <see cref="Configuration"/> to <paramref name="services"/>.
        /// </summary>
        /// <param name="config">Replaces the existing config.</param>
        /// <returns></returns>
        public static IServiceCollection AddConfiguration(this IServiceCollection services, Configuration config = default)
        {
            if (config == default && File.Exists("./Config.json"))
            {
                var read = File.ReadAllBytes("./Config.json");
                config = JsonSerializer.Parse<Configuration>(read);
            }
            else if (config != default && !File.Exists("./Config.json"))
            {
                BuildConfig();
            }
            else
            {
                config = new Configuration
                {
                    Host = "127.0.0.1",
                    Port = 6666,
                    LogLevel = LogLevel.None,
                    Password = "frostbyte",
                    RatelimitPolicy = new RatelimitPolicy
                    {
                        IsEnabled = true,
                        PerSecond = 5,
                        PerMinute = 69,
                        PerHour = 420,
                        PerDay = 1447
                    },
                    Sources = new MediaSources
                    {
                        EnableLocal = true,
                        EnableSoundCloud = true,
                        EnableYouTube = true
                    }
                };
                BuildConfig();
            }

            void BuildConfig()
            {
                var data = JsonSerializer.ToUtf8Bytes(config, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllBytes("./Config.json", data);
            }

            return services.AddSingleton(config);
        }
    }
}