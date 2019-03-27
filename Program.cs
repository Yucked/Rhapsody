using Frostbyte.Extensions;
using Frostbyte.Handlers;
using Frostbyte.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
                .AddLogging(x =>
                {
                    x.ClearProviders();
                    x.AddProvider(new LogProvider());
                })
                .AddAttributeServices();

            Provider = services.BuildServiceProvider();
        }

        public static Task Main(string[] args)
        {
            // TODO: Do something with custom arguments?

            return new Program().InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            Console.WriteAscii($"   {nameof(Frostbyte)}".ToUpper(), Color.Cyan);

            var configHandler = Provider.GetRequiredService<ConfigHandler>();
            var config = configHandler.ValidateConfiguration();

            var wsHandler = Provider.GetRequiredService<WebsocketHandler>();
            wsHandler.Initialize(config);

            await Task.Delay(-1);
        }

        public ValueTask DisposeAsync()
        {
            // TODO: Dispose of websocket connections
            return default;
        }
    }
}