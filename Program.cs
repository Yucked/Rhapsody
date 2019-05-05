using Frostbyte.Extensions;
using Frostbyte.Handlers;
using Frostbyte.Websocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Drawing;
using System.Threading.Tasks;
using Console = Colorful.Console;

namespace Frostbyte
{
    public sealed class Program : IAsyncDisposable
    {
        public Program()
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
            Console.WriteAscii($"   {nameof(Frostbyte)}".ToUpper(), Color.Cyan);

            Provider.InjectRequiredServices();

            var cf = Provider.GetRequiredService<ConfigHandler>();
            var config = cf.ValidateConfiguration();

            var wsServer = Provider.GetRequiredService<WsServer>();
            await wsServer.InitializeAsync(config);

            await Task.Delay(-1);
        }
    }
}