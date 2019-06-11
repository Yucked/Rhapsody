using Frostbyte.Entities;
using Frostbyte.Entities.Audio;
using Frostbyte.Entities.Results;
using System.IO;
using System.Threading.Tasks;

namespace Frostbyte.Sources
{
    public abstract class SourceBase
    {
        public abstract string Prefix { get; }
        public AudioSources AudioSources { get; }

        public SourceBase(Configuration config)
        {
            AudioSources = config.Sources;
        }

        public abstract ValueTask<SearchResult> SearchAsync(string query);

        public abstract ValueTask<Stream> GetStreamAsync(string id);

        public ValueTask<Stream> GetStreamAsync(AudioTrack track)
            => GetStreamAsync(track.Id);
    }
}