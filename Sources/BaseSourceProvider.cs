using System.IO;
using System.Threading.Tasks;
using Frostbyte.Entities.Audio;
using Frostbyte.Entities.Responses;

namespace Frostbyte.Sources
{
    public abstract class BaseSourceProvider
    {
        /// <summary>
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public abstract ValueTask<SearchResponse> SearchAsync(string query);

        /// <summary>
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        protected abstract ValueTask<Stream> GetStreamAsync(string query);

        /// <summary>
        /// </summary>
        /// <param name="track"></param>
        /// <returns></returns>
        public ValueTask<Stream> GetStreamAsync(AudioTrack track)
        {
            return GetStreamAsync(track.Id);
        }
    }
}