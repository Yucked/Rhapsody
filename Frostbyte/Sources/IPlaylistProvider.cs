using System.Threading;
using System.Threading.Tasks;
using Frostbyte.Entities.Audio;

namespace Frostbyte.Sources
{
    public interface IPlaylistProvider
    {
        ValueTask<Playlist> GetPlaylistAsync(
            string id,
            CancellationToken cancellationToken = default);
    }
}