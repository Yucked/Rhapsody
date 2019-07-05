using System.IO;
using System.Threading.Tasks;
using Frostbyte.Entities.Responses;

namespace Frostbyte.Sources
{
    public sealed class AppleMusicSource : BaseSourceProvider
    {
        public override ValueTask<SearchResponse> SearchAsync(string query)
        {
            return default;
        }

        protected override ValueTask<Stream> GetStreamAsync(string id)
        {
            return default;
        }
    }
}