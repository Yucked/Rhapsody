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
        private readonly Configuration _config;
        private readonly WSServer _wSServer;
        private readonly HttpHandler _httpHandler;

        public MainHandler()
        {
            _config = BuildConfiguration();
            Singleton.Add<Configuration>(_config);
            Singleton.Add<HttpHandler>();
            Singleton.Add<CacheHandler>();
            Singleton.Add<SourceHandler>();
            Singleton.Add<WSServer>();

            _httpHandler = Singleton.Of<HttpHandler>();
            _wSServer = Singleton.Of<WSServer>();
        }

        public async Task InitializeAsync()
        {
            PrintHeader();
            await VerifyConnectionAsync().ConfigureAwait(false);

            PrintHeader();
            await PrintRepositoryInformationAsync().ConfigureAwait(false);
            Console.WriteLine(new string('-', 100), Color.Gray);
            PrintSystemInformation();
            Console.WriteLine(new string('-', 100), Color.Gray);

            Singleton.Of<SourceHandler>().Initialize();
            await _wSServer.InitializeAsync().ConfigureAwait(false);

            await Task.Delay(-1);
        }

        private async Task VerifyConnectionAsync()
        {
            var isReady = false;
            int tries = 0, waitTime = 0;

            LogHandler<MainHandler>.Log.Information("Verifying internet connectivity before proceeding.");

            while (!isReady && tries < _config.MaxConnectionRetries)
            {
                var ping = await _httpHandler.PingAsync().ConfigureAwait(false);
                if (ping)
                {
                    isReady = ping;
                    LogHandler<MainHandler>.Log.Information("Internet connection verified successfully! Continuing ...");
                    Console.Clear();
                }
                else
                {
                    tries++;
                    waitTime += _config.ReconnectInterval;
                    LogHandler<MainHandler>.Log.Warning($"Attempt #{tries}, next attempt in {TimeSpan.FromMilliseconds(waitTime).Seconds}s.");
                    await Task.Delay(waitTime).ConfigureAwait(false);
                }
            }
        }

        private void PrintHeader()
        {
            const string header = @"

        ▄████  █▄▄▄▄ ████▄    ▄▄▄▄▄      ▄▄▄▄▀ ███ ▀▄    ▄   ▄▄▄▄▀ ▄███▄   
        █▀   ▀ █  ▄▀ █   █   █     ▀▄ ▀▀▀ █    █  █  █  █ ▀▀▀ █    █▀   ▀  
        █▀▀    █▀▀▌  █   █ ▄  ▀▀▀▀▄       █    █ ▀ ▄  ▀█      █    ██▄▄    
        █      █  █  ▀████  ▀▄▄▄▄▀       █     █  ▄▀  █      █     █▄   ▄▀ 
         █       █                      ▀      ███  ▄▀      ▀      ▀███▀   
          ▀     ▀                                                          ";

            Console.WriteLine(header, Color.FromArgb(36, 231, 96));
        }

        private async Task PrintRepositoryInformationAsync()
        {
            var result = new GitHubResult();
            var getBytes = await _httpHandler
                .GetBytesAsync("https://api.github.com/repos/Yucked/Frostbyte").ConfigureAwait(false);
            result.Repo = JsonSerializer.Parse<GitHubRepo>(getBytes.Span);

            getBytes = await _httpHandler
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
                    VoiceSettings = VoiceSettings.Music,
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