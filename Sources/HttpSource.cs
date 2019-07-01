using System.IO;
using System.Threading.Tasks;
using Frostbyte.Entities.Responses;

namespace Frostbyte.Sources
{
    public sealed class HttpSource : BaseSourceProvider
    {
        public override ValueTask<SearchResponse> SearchAsync(string query)
        {
            return default;
        }

        public override ValueTask<Stream> GetStreamAsync(string id)
        {
            return default;
        }
    }
}