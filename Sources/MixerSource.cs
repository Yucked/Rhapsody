using System;
using System.IO;
using System.Threading.Tasks;
using Frostbyte.Entities.Results;

namespace Frostbyte.Sources
{
    public sealed class MixerSource : ISourceProvider
    {
        public ValueTask<SearchResult> SearchAsync(string query)
        {
            throw new NotImplementedException();
        }

        public ValueTask<Stream> GetStreamAsync(string id)
        {
            throw new NotImplementedException();
        }
    }
}