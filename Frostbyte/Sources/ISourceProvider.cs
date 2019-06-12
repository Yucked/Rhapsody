using Frostbyte.Entities.Audio;
using Frostbyte.Entities.Results;
using System.IO;
using System.Threading.Tasks;

namespace Frostbyte.Sources
{
    public interface ISourceProvider
    {
        ValueTask<SearchResult> SearchAsync(string query);

        ValueTask<Stream> GetStreamAsync(string query);

        public ValueTask<Stream> GetStreamAsync(AudioTrack track)
            => GetStreamAsync(track.Id);
    }
}