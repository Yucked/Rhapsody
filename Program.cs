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
        private IServiceProvider Provider { get; }

        public Program()
        {
            var services = new ServiceCollection()
                .AddAttributeServices();

            Provider = services.BuildServiceProvider();
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

            var wsServer = Provider.GetRequiredService<WSServer>();
            await wsServer.InitializeAsync(config);

            await Task.Delay(-1);
        }

        public ValueTask DisposeAsync()
        {
            // TODO: Dispose of websocket connections
            return default;
        }
    }
}