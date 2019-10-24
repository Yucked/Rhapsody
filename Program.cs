using System.IO;
using System.Text.Json;
using Concept.Entities.Options;
using Concept.Logger;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Concept
{
    public readonly struct Program
    {
        public static void Main()
        {
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
}