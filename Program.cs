using Concept.Configuration;
using Concept.Logger;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Concept
{
    public readonly struct Program
    {
        public static void Main()
            => Host.CreateDefaultBuilder(default)
                .ConfigureWebHostDefaults(hostBuilder =>
                {
                    hostBuilder.UseStartup(typeof(Startup));

                    //I don't see any form to pass the config in the startup
                    var config = new ConfigurationLoader().GetConfiguration();
                    hostBuilder.UseUrls($"http://{config.Host}:{config.Port}");
                })
                .ConfigureLogging((hostBuilder, logging) =>
                {
                    var section = hostBuilder.Configuration.GetSection("Logging");
                    logging.ClearProviders();
                    logging.AddProvider(new ModifiedProvider(section));
                })
                .Build()
                .Run();
    }
}