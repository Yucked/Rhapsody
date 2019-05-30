using Frostbyte.Extensions;
using Frostbyte.Handlers;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace Frostbyte
{
    public sealed class Program
    {
        public static Task Main()
        {
            return new Program().InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            var services = new ServiceCollection().AddAttributeServices();
            var provider = services.BuildServiceProvider();

            await provider.GetRequiredService<StartupHandler>().InitializeAsync();

            await Task.Delay(-1);
        }
    }
}