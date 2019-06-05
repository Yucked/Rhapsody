using System.Threading;
using System.Threading.Tasks;
using Frostbyte.Entities.Audio;

namespace Frostbyte.Sources
{
    public interface ITrackProvider : ISourceProvider
    {
        bool ValidateTrack(string id);

        ValueTask<Track> GetTrackAsync(
            string id,
            CancellationToken token = default);
    }
}