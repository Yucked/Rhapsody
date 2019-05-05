using Frostbyte.Attributes;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Reflection;

namespace Frostbyte.Extensions
{
    public static class DependencyExtensions
    {
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
                        services.AddScoped(match);
                        break;

                    case ServiceLifetime.Singleton:
                        services.AddSingleton(match);
                        break;

                    case ServiceLifetime.Transient:
                        services.AddTransient(match);
                        break;
                }
            }

            return services;
        }

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
    }
}