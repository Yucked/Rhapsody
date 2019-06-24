using Frostbyte.Entities;
using Frostbyte.Entities.Enums;
using Console = Colorful.Console;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Colorful;
using Frostbyte.Entities.Results;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using Frostbyte.Websocket;
using System;

namespace Frostbyte.Handlers
{
    public sealed class MainHandler
    {
        public async Task InitializeAsync()
        {
            Singleton.Add<HttpHandler>();
            Singleton.Add<CacheHandler>();
            var config = BuildConfiguration();
            Singleton.Add<Configuration>(config);

            await PrintRepositoryInformationAsync().ConfigureAwait(false);
            Console.WriteLine(new string('-', 100), Color.Gray);
            PrintSystemInformation();
            Console.WriteLine(new string('-', 100), Color.Gray);

            Singleton.Of<SourceHandler>().Initialize();
            await Singleton.Of<WSServer>().InitializeAsync().ConfigureAwait(false);

            await Task.Delay(-1);
        }

        private async Task PrintRepositoryInformationAsync()
        {
            var result = new GitHubResult();
            var getBytes = await Singleton.Of<HttpHandler>()
                .GetBytesAsync("https://api.github.com/repos/Yucked/Frostbyte").ConfigureAwait(false);
            result.Repo = JsonSerializer.Parse<GitHubRepo>(getBytes.Span);

            getBytes = await Singleton.Of<HttpHandler>()
                .WithUrl("https://api.github.com/repos/Yucked/Frostbyte/commits")
                .GetBytesAsync().ConfigureAwait(false);
            result.Commit = JsonSerializer.Parse<IEnumerable<GitHubCommit>>(getBytes.Span).FirstOrDefault();

            Console.WriteLineFormatted($"    {{0}}: {result.Repo.OpenIssues} opened   |    {{1}}: {result.Repo.License.Name}    | {{2}}: {result.Commit?.SHA}",
                                       Color.White,
                                       new Formatter("Issues", Color.Plum),
                                       new Formatter("License", Color.Plum),
                                       new Formatter("SHA", Color.Plum));
        }

        private void PrintSystemInformation()
        {
            var process = Process.GetCurrentProcess();
            Console.WriteLineFormatted(
                $"    {{0}}: {RuntimeInformation.FrameworkDescription}    |    {{1}}: {RuntimeInformation.OSDescription}\n" +
                "    {2}: {3} ID / Using {4} Threads / Started On {5}",
                                       Color.White,
                                       new Formatter("FX Info", Color.Crimson),
                                       new Formatter("OS Info", Color.Crimson),
                                       new Formatter("Process", Color.Crimson),
                                       new Formatter(process.Id, Color.GreenYellow),
                                       new Formatter(process.Threads.Count, Color.GreenYellow),
                                       new Formatter($"{process.StartTime:MMM d - hh:mm:ss tt}", Color.GreenYellow));
        }

        private Configuration BuildConfiguration()
        {
            Configuration config;
            if (File.Exists("./Config.json"))
            {
                var read = File.ReadAllBytes("./Config.json");
                config = JsonSerializer.Parse<Configuration>(read);
            }
            else
            {
                config = new Configuration
                {
                    Host = "127.0.0.1",
                    Port = 6666,
                    LogLevel = LogLevel.None,
                    Password = "frostbyte",
                    MaxConnectionRetries = 10,
                    ReconnectInterval = 5000,
                    Sources = new AudioSources
                    {
                        EnableLocal = true,
                        EnableHttp = true,
                        EnableSoundCloud = true,
                        EnableYouTube = true
                    }
                };

                var data = JsonSerializer.ToUtf8Bytes(config, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllBytes("./Config.json", data);
            }

            return config;
        }
    }
}