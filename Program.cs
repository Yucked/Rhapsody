using Frostbyte.Extensions;
using Frostbyte.Handlers;
using Frostbyte.Websocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Drawing;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Console = Colorful.Console;

namespace Frostbyte
{
    public sealed class Program : IAsyncDisposable
    {
        private Program()
        {
            var services = new ServiceCollection().AddAttributeServices();

            Provider = services.BuildServiceProvider();
        }

        private IServiceProvider Provider { get; }

        public ValueTask DisposeAsync()
        {
            // TODO: Dispose of websocket connections
            return default;
        }

        public static Task Main(string[] arguments)
        {
            // TODO: Do something with custom arguments?

            return new Program().InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            PrintInformationHeader();
            Provider.InjectRequiredServices();

            var cf = Provider.GetRequiredService<ConfigHandler>();
            var config = cf.ValidateConfiguration();

            var wsServer = Provider.GetRequiredService<WsServer>();
            await wsServer.InitializeAsync(config);

            await Task.Delay(-1);
        }

        private void PrintInformationHeader()
        {
            const string header = @"        ▄████  █▄▄▄▄ ████▄    ▄▄▄▄▄      ▄▄▄▄▀ ███ ▀▄    ▄   ▄▄▄▄▀ ▄███▄   
        █▀   ▀ █  ▄▀ █   █   █     ▀▄ ▀▀▀ █    █  █  █  █ ▀▀▀ █    █▀   ▀  
        █▀▀    █▀▀▌  █   █ ▄  ▀▀▀▀▄       █    █ ▀ ▄  ▀█      █    ██▄▄    
        █      █  █  ▀████  ▀▄▄▄▄▀       █     █  ▄▀  █      █     █▄   ▄▀ 
         █       █                      ▀      ███  ▄▀      ▀      ▀███▀   
          ▀     ▀                                                          ";
            var lineBreak = new string('-', 90);
            var informationalVersion =
                ((AssemblyInformationalVersionAttribute[])
                    typeof(Uri).Assembly.GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute), false))[0]
                .InformationalVersion;

            Console.WriteLine(header, Color.Teal);
            Console.WriteLine(lineBreak, Color.Gray);
            Console.Write("    CoreFX Build: ", Color.Plum);
            Console.Write($"{informationalVersion.Split('+')[0]} ({informationalVersion.Split('+')[1]})");
            Console.Write(Environment.NewLine);
            Console.WriteLine(lineBreak, Color.Gray);
        }
    }
}