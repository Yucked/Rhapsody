using System.Drawing;
using System.Threading.Tasks;
using Colorful;
using Frostbyte.Audio;
using Frostbyte.Audio.Codecs;
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
            Singleton.Add<HttpFactory>();
            Singleton.Add<FFmpegPipe>();

            LogFactory.PrintHeader();
            await LogFactory.PrintRepositoryInformationAsync()
                .ConfigureAwait(false);
            Console.WriteLine(new string('-', 100), Color.Gray);
            LogFactory.PrintSystemInformation();
            Console.WriteLine(new string('-', 100), Color.Gray);

            var configFactory = Singleton.Of<ConfigFactory>();
            configFactory.BuildConfigAsync();
            var config = await configFactory.LoadConfigAsync()
                .ConfigureAwait(false);

            Singleton.Add(config);
            Singleton.Add<SourceFactory>();
            Singleton.Add<WebsocketServer>();

            OpusCodec.OpusVoiceType = config.Audio.OpusVoiceType;

            var source = Singleton.Of<SourceFactory>();
            source.CreateSources();

            var server = Singleton.Of<WebsocketServer>();
            await server.InitializeAsync()
                .ConfigureAwait(false);

            await Task.Delay(-1);
        }
    }
}