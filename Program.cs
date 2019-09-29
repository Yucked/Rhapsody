using System.IO;
using System.Text.Json;
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
            Settings settings;

            if (!File.Exists("Config.json"))
            {
                settings = Settings.CreateDefault();
                var raw = JsonSerializer.SerializeToUtf8Bytes(settings);
                File.WriteAllBytes("Config.json", raw);
            }
            else
            {
                var raw = File.ReadAllBytes("Config.json");
                settings = JsonSerializer.Deserialize<Settings>(raw);
            }

            Host.CreateDefaultBuilder(default)
                .ConfigureAppConfiguration(configBuilder =>
                {
                    configBuilder.SetBasePath(Directory.GetCurrentDirectory());
                    configBuilder.AddJsonFile("Config.json", false, true);
                })
                .ConfigureWebHostDefaults(hostBuilder =>
                {
                    hostBuilder.UseStartup<Startup>();
                    hostBuilder.UseUrls($"http://{settings.Hostname}:{settings.Port}");
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