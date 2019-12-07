using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Text.Json;
using Concept.Caches;
using Concept.Controllers;
using Concept.Entities;
using Concept.Entities.Options;
using Concept.Jobs;
using Concept.Logger;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Theory;

namespace Concept
{
    public readonly struct Program
    {
        public static void Main()
        {
            Extensions.PrintHeaderAndInformation();
            ApplicationOptions applicationOptions;

            if (!File.Exists("options.json"))
            {
                applicationOptions = ApplicationOptions.Default;
                var raw = JsonSerializer.SerializeToUtf8Bytes(applicationOptions);
                File.WriteAllBytes("options.json", raw);
            }
            else
            {
                var raw = File.ReadAllBytes("options.json");
                applicationOptions = JsonSerializer.Deserialize<ApplicationOptions>(raw);
            }

            Host.CreateDefaultBuilder(default)
                .ConfigureAppConfiguration(configBuilder =>
                {
                    configBuilder.SetBasePath(Directory.GetCurrentDirectory());
                    configBuilder.AddJsonFile("options.json", false, true);
                })
                .ConfigureWebHostDefaults(hostBuilder =>
                {
                    hostBuilder.UseStartup<Startup>();
                    hostBuilder.UseUrls($"http://{applicationOptions.Hostname}:{applicationOptions.Port}");
                })
                .ConfigureServices((builder, services) =>
                {
                    var config = builder.Configuration;
                    var options = config.Get<ApplicationOptions>();

                    //Adding required Concept Services
                    services.AddTransient<ClientsCache>();
                    services.AddSingleton<Theoretical>();
                    services.AddSingleton<WebSocketController>();
                    services.AddControllers();

                    //Adding Concept Background Workers
                    services.AddHostedService<MetricsJob>();
                    services.AddHostedService<LogService>();

                    builder.Properties.Add("LogService", services.First(x => x.ImplementationType == typeof(LogService)).ImplementationInstance);
                    //Adding other stuff
                    services.Configure<ApplicationOptions>(config);
                    services.AddAuthentication("HeaderAuth")
                        .UseHeaderAuthentication(options => options.Authorization = options.Authorization);

                    //Adding optional cache related items
                    if (options.CacheOptions.Limit > 0)
                    {
                        services.AddHostedService<PurgeJob>();
                        services.AddSingleton<ResponsesCache>();
                    }
                })
                .ConfigureLogging((hostBuilder, logging) =>
                { 
                    var section = hostBuilder.Configuration.GetSection("Logging");

                    logging.ClearProviders();
                    logging.AddProvider(new ModifiedProvider(section, (LogService)hostBuilder.Properties["LogService"]));
                })
                .Build()
                .Run();
        }
    }
}