using System;
using System.IO;
using System.Threading.Tasks;
using Frostbyte.Entities.Responses;

namespace Frostbyte.Sources
{
    public sealed class VimeoSource : BaseSourceProvider
    {
        public override ValueTask<SearchResponse> SearchAsync(string query)
        {
            throw new NotImplementedException();
        }

        protected override ValueTask<Stream> GetStreamAsync(string id)
        {
            throw new NotImplementedException();
        }
    }
}