using System.IO;
using System.Threading.Tasks;
using Frostbyte.Entities;
using Frostbyte.Entities.Infos;
using Frostbyte.Factories;

namespace Frostbyte.Sources
{
    public abstract class BaseSource
    {
        protected readonly HttpFactory HttpFactory;

        protected BaseSource()
        {
            HttpFactory = Singleton.Of<HttpFactory>();
        }

        public abstract ValueTask<SearchResponse> SearchAsync(string query);

        public abstract ValueTask<Stream> GetStreamAsync(string trackId);

        public ValueTask<Stream> GetStreamAsync(TrackInfo track)
        {
            return GetStreamAsync(track.Id);
        }
    }
}