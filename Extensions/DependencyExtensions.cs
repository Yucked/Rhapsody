using Frostbyte.Attributes;
using Microsoft.Extensions.DependencyInjection;
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
    }
}