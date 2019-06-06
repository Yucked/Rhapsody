using System.IO;
using System.Threading.Tasks;
using Frostbyte.Attributes;
using Frostbyte.Entities;
using Frostbyte.Entities.Audio;

namespace Frostbyte.Sources
{
    [RegisterService(typeof(ISourceProvider))]
    public sealed class TwitchSource : ISourceProvider
    {
        public string Prefix { get; }
        public bool IsEnabled { get; }

        public TwitchSource(Configuration config)
        {
            Prefix = "twsearch";
            IsEnabled = config.Sources.EnableTwitch;
        }

        public ValueTask<RESTEntity> SearchAsync(string query)
        {
            throw new System.NotImplementedException();
        }

        public ValueTask<Track> GetTrackAsync(string id)
        {
            throw new System.NotImplementedException();
        }

        public ValueTask<Stream> GetStreamAsync(string id)
        {
            throw new System.NotImplementedException();
        }

        public ValueTask<Stream> GetStreamAsync(Track track)
        {
            throw new System.NotImplementedException();
        }
    }
}