using Frostbyte.Attributes;
using Frostbyte.Entities;
using Frostbyte.Entities.Enums;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json.Serialization;

namespace Frostbyte.Extensions
{
    public static class DependencyExtensions
    {
        /// <summary>
        /// Adds all classes that have <see cref="ServiceAttribute"/> declared.
        /// </summary>
        public static IServiceCollection AddAttributeServices(this IServiceCollection services)
        {
            var matches = Assembly.GetExecutingAssembly().GetTypes().Where(x => x.GetCustomAttribute<ServiceAttribute>() != null).ToArray();

            if (matches.Length is 0)
                return services;

            foreach (var match in matches)
            {
                var attr = match.GetCustomAttribute<ServiceAttribute>();
                switch (attr.Lifetime)
                {
                    case ServiceLifetime.Scoped:
                        if (attr.BaseType != null)
                        {
                            services.AddScoped(attr.BaseType, match);
                        }
                        else
                        {
                            services.AddScoped(match);
                        }

                        break;

                    case ServiceLifetime.Singleton:
                        if (attr.BaseType != null)
                        {
                            services.AddSingleton(attr.BaseType, match);
                        }
                        else
                        {
                            services.AddSingleton(match);
                        }

                        break;

                    case ServiceLifetime.Transient:
                        if (attr.BaseType != null)
                        {
                            services.AddTransient(attr.BaseType, match);
                        }
                        else
                        {
                            services.AddTransient(match);
                        }

                        break;
                }
            }

            return services;
        }

        /*
        public static IServiceProvider InjectRequiredServices(this IServiceProvider provider)
        {
            var matches = Assembly.GetExecutingAssembly().GetTypes().Where(x => x.GetCustomAttribute<ServiceAttribute>() != null).ToArray();

            if (matches.Length is 0)
                return provider;

            foreach (var match in matches)
            {
                var attr = match.GetCustomAttribute<ServiceAttribute>();
                if (attr.InjectableTypes is null || attr.InjectableTypes.Length is 0)
                    continue;

                var properties = match.GetRuntimeProperties().ToArray();

                foreach (var type in attr.InjectableTypes)
                {
                    var service = provider.GetService(type);
                    if (service is null)
                        continue;

                    var property = properties.FirstOrDefault(x => x.PropertyType == type);
                    property?.SetValue(match, service);
                }
            }

            return provider;
        }
        */

        /// <summary>
        /// Adds <see cref="ConfigEntity"/> to <paramref name="services"/>.
        /// </summary>
        /// <param name="config">Replaces the existing config.</param>
        /// <returns></returns>
        public static IServiceCollection AddConfiguration(this IServiceCollection services, ConfigEntity config = default)
        {
            if (config == default && File.Exists("./Config.json"))
            {
                var read = File.ReadAllBytes("./Config.json");
                config = JsonSerializer.Parse<ConfigEntity>(read);
            }
            else if (config != default && !File.Exists("./Config.json"))
            {
                BuildConfig();
            }
            else
            {
                config = new ConfigEntity
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