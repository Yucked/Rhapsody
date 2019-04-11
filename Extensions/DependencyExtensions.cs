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
            var matches = Assembly.GetExecutingAssembly().GetTypes()
                .Where(x => x.GetCustomAttribute<ServiceAttribute>() != null).ToArray();

            if (matches.Length is 0)
                return services;

            foreach (var match in matches)
            {
                var attr = match.GetCustomAttribute<ServiceAttribute>();
                _ = attr.Lifetime switch
                {
                    ServiceLifetime.Scoped
                        => services.AddScoped(match),

                    ServiceLifetime.Singleton
                        => services.AddSingleton(match),

                    ServiceLifetime.Transient
                        => services.AddTransient(match)
                };
            }

            return services;
        }

        public static IServiceProvider InjectRequiredServices(this IServiceProvider provider)
        {
            var matches = Assembly.GetExecutingAssembly().GetTypes()
                .Where(x => x.GetCustomAttribute<ServiceAttribute>() != null).ToArray();

            if (matches.Length is 0)
                return provider;

            foreach (var match in matches)
            {
                var attr = match.GetCustomAttribute<ServiceAttribute>();
                if (attr.InjectableTypes is null || attr.InjectableTypes.Length is 0)
                    continue;

                var properties = match.GetRuntimeProperties();

                foreach (var type in attr.InjectableTypes)
                {
                    var service = provider.GetService(type);
                    if (service is null)
                        continue;

                    var property = properties.FirstOrDefault(x => x.PropertyType == type);
                    if (property is null)
                        continue;

                    property.SetValue(match, service);
                }
            }

            return provider;
        }
    }
}