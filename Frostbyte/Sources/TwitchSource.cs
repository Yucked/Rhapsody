using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Frostbyte.Entities;
using Frostbyte.Entities.Audio;

namespace Frostbyte.Sources
{
    public sealed class TwitchSource : ISearchProvider, IStreamProvider
    {
        public string Prefix => "twsearch";

        public bool IsEnabled { get; }

        public TwitchSource(ConfigEntity config)
        {
            IsEnabled = config.Sources.EnableTwitch;
        }
        
        public ValueTask<RESTEntity> SearchAsync(string query, CancellationToken token = default)
        {
            throw new System.NotImplementedException();
        }

        public ValueTask<Stream> GetStreamAsync(IAudioItem audioItem, CancellationToken token = default)
        {
            throw new System.NotImplementedException();
        }

        public ValueTask<Stream> GetStreamAsync(string id, CancellationToken token = default)
        {
            throw new System.NotImplementedException();
        }
    }
}