using Frostbyte.Extensions;
using Frostbyte.Factories;
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

            var cf = Provider.GetRequiredService<ConfigFactory>();
            var config = cf.ValidateConfiguration();

            var wf = Provider.GetRequiredService<WebsocketFactory>();
            wf.Initialize(config);

            await Task.Delay(-1);
        }

        public ValueTask DisposeAsync()
        {
            // TODO: Dispose of websocket connections
            return default;
        }
    }
}