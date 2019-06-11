
using Frostbyte.Entities;
using Frostbyte.Entities.Enums;
using Frostbyte.Handlers;
using Frostbyte.Sources;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;

namespace Frostbyte.Extensions
{
    public static class DependencyExtensions
    {
        public static IServiceCollection RegisterSources(this IServiceCollection services)
        {
            var matches = Assembly.GetExecutingAssembly().GetTypes().Where(x => x.IsSubclassOf(typeof(SourceBase))).ToArray();

            if (matches.Length is 0)
                return services;

            foreach (var match in matches)
            {
                services.AddSingleton(match);

                LogHandler<IServiceCollection>.Log.Debug($"Registered {match.Name} as singleton.");
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
                LogHandler<IServiceCollection>.Log.Information("Loaded configuration.");
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
                    Sources = new AudioSources
                    {
                        EnableLocal = true,
                        EnableHttp = true,
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
                LogHandler<IServiceCollection>.Log.Information("Built new configuration.");
            }

            services.AddSingleton(config);
            LogHandler<IServiceCollection>.Log.Debug($"Registered {nameof(Configuration)} as singleton.");
            return services;
        }
    }
}