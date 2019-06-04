using System.IO;
using System.Threading.Tasks;
using Frostbyte.Entities;

namespace Frostbyte.Sources
{
    public sealed class TwitchSource : ISource
    {
        public string Prefix { get; }
        public bool IsEnabled { get; }

        public TwitchSource(ConfigEntity config)
        {
            Prefix = "twsearch";
            IsEnabled = config.Sources.EnableTwitch;
        }

        public async ValueTask<RESTEntity> PrepareResponseAsync(string query)
        {
            throw new System.NotImplementedException();
        }

        public async ValueTask<Stream> GetStreamAsync(string id)
        {
            throw new System.NotImplementedException();
        }

        public async ValueTask<Stream> GetStreamAsync(TrackEntity track)
        {
            throw new System.NotImplementedException();
        }
    }
}