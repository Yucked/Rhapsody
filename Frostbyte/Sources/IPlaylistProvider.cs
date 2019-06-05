using System.Threading;
using System.Threading.Tasks;
using Frostbyte.Entities.Audio;

namespace Frostbyte.Sources
{
    public interface IPlaylistProvider : ISourceProvider 
    {
        ValueTask<Playlist> GetPlaylistAsync(
            string id,
            CancellationToken token = default);
    }
}