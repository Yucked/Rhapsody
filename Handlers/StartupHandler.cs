using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Colorful;
using Frostbyte.Attributes;
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
            PrintInformationHeader();
            var config = _config.ValidateConfiguration();
            await _server.InitializeAsync(config).ConfigureAwait(false);
        }

        private void PrintInformationHeader()
        {
            const string header = @"


        ▄████  █▄▄▄▄ ████▄    ▄▄▄▄▄      ▄▄▄▄▀ ███ ▀▄    ▄   ▄▄▄▄▀ ▄███▄   
        █▀   ▀ █  ▄▀ █   █   █     ▀▄ ▀▀▀ █    █  █  █  █ ▀▀▀ █    █▀   ▀  
        █▀▀    █▀▀▌  █   █ ▄  ▀▀▀▀▄       █    █ ▀ ▄  ▀█      █    ██▄▄    
        █      █  █  ▀████  ▀▄▄▄▄▀       █     █  ▄▀  █      █     █▄   ▄▀ 
         █       █                      ▀      ███  ▄▀      ▀      ▀███▀   
          ▀     ▀                                                          ";
            var lineBreak = $"\n{new string('-', 90)}\n";
            var process = Process.GetCurrentProcess();

            Console.WriteLine(header, Color.Teal);
            Console.WriteLine(lineBreak, Color.Gray);
            Console.Write("     Runtime: ", Color.Plum);
            Console.Write($"{RuntimeInformation.FrameworkDescription}\n");
            Console.Write("     Process: ", Color.Plum);
            Console.Write($"{process.Id} ID | {process.Threads.Count} Threads\n");
            Console.WriteLine(lineBreak, Color.Gray);
        }
    }
}