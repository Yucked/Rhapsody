using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Frostbyte.Entities.Audio;

namespace Frostbyte.Sources
{
    public interface IStreamProvider : ISourceProvider
    {
        ValueTask<Stream> GetStreamAsync(
            IAudioItem audioItem,
            CancellationToken token = default);
        
        ValueTask<Stream> GetStreamAsync(
            string id,
            CancellationToken token = default);
    }
}