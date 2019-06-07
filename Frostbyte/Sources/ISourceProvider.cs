using Frostbyte.Entities.Audio;
using Frostbyte.Entities.Results;
using System.IO;
using System.Threading.Tasks;

namespace Frostbyte.Sources
{
    public interface ISourceProvider
    {
        string Prefix { get; }

        bool IsEnabled { get; }

        ValueTask<SearchResult> SearchAsync(string query);

        ValueTask<Stream> GetStreamAsync(string id);

        ValueTask<Stream> GetStreamAsync(AudioTrack track)
            => GetStreamAsync(track.Id);
    }
}