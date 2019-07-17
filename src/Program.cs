using System.Threading.Tasks;
using Frostbyte.Factories;
using Frostbyte.Server;

namespace Frostbyte
{
    internal sealed class Program
    {
        private static async Task Main()
        {
            await new Program().InitializeAsync()
                .ConfigureAwait(false);

            await Task.Delay(-1)
                .ConfigureAwait(false);
        }

        private async Task InitializeAsync()
        {
            Singleton.Add<ConfigFactory>();

            var configFactory = Singleton.Of<ConfigFactory>();
            configFactory.BuildConfigAsync();
            var config = await configFactory.LoadConfigAsync()
                .ConfigureAwait(false);

            Singleton.Add(config);
            Singleton.Add<HttpFactory>();
            Singleton.Add<SourceFactory>();
            Singleton.Add<WebsocketServer>();

            var source = Singleton.Of<SourceFactory>();
            source.CreateSources();

            var server = Singleton.Of<WebsocketServer>();
            await server.InitializeAsync()
                .ConfigureAwait(false);

            await Task.Delay(-1);
        }
    }
}