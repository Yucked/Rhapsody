using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Colorful;
using Frostbyte.Attributes;
using Frostbyte.Entities.Results;
using Frostbyte.Websocket;
using Microsoft.Extensions.DependencyInjection;

namespace Frostbyte.Handlers
{
    [Service(ServiceLifetime.Singleton)]
    public sealed class StartupHandler
    {
        private readonly ConfigHandler _config;
        private readonly WsServer _server;

        public StartupHandler(ConfigHandler config, WsServer wsServer)
        {
            _config = config;
            _server = wsServer;
        }

        public async Task InitializeAsync()
        {
            SetupConsole();
            PrintHeader();
            await PrintRepositoryInformationAsync().ConfigureAwait(false);
            PrintSystemInformation();
            
            var config = _config.ValidateConfiguration();
            await _server.InitializeAsync(config).ConfigureAwait(false);
        }

        private void SetupConsole()
        {
            Console.Title = "Frostbyte - Yucked";
            Console.WindowHeight = 25;
            Console.WindowWidth = 140;
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

            Console.WriteLine(header, Color.Teal);
        }
        
        private async Task PrintRepositoryInformationAsync()
        {
            var result = new GitHubResult();
            var get = await HttpHandler.Instance.GetBytesAsync("https://api.github.com/repos/Yucked/Frostbyte/").ConfigureAwait(false);
            result.Repo = JsonSerializer.Parse<GitHubRepo>(get.Span);

            get = await HttpHandler.Instance.GetBytesAsync("https://api.github.com/repos/Yucked/Frostbyte/commits").ConfigureAwait(false);
            result.Commit = JsonSerializer.Parse<IEnumerable<GitHubCommit>>(get.Span).FirstOrDefault();
            result.Commit.Sha.Substring(0, 7);
            
            Console.WriteLineFormatted($"    {{0}}: {result.Repo.OpenIssues}    |    {{1}}: {result.Repo.License.Name}    | {{2}}: {result.Commit.Sha}",
                                       Color.White, 
                                       new Formatter("", Color.Plum),
                                       new Formatter("Open Issues", Color.Plum),
                                       new Formatter("License Name", Color.Plum),
                                       new Formatter("Current Version", Color.Plum));
        }

        private void PrintSystemInformation()
        {
            var process = Process.GetCurrentProcess();
            Console.WriteLineFormatted(
                                       $"    {{0}}: {RuntimeInformation.FrameworkDescription}    |    {{1}}: {RuntimeInformation.OSDescription}\n" +
                                       $"    {{2}}: {process.Id} ID / Using {{3}} Threads / Started On {{4}}\n",
                                       Color.White,
                                       new Formatter("FX Info", Color.Gold),
                                       new Formatter("OS Info", Color.Gold),
                                       new Formatter("Process", Color.Gold),
                                       new Formatter(process.Id, Color.GreenYellow),
                                       new Formatter($"{process.StartTime:MMM d - hh:mm:ss tt}", Color.Gold));
        }
    }
}