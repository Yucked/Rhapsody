using System;
using System.IO;
using System.Threading.Tasks;
using Frostbyte.Entities;
using Frostbyte.Entities.Results;

namespace Frostbyte.Sources
{
    public sealed class AppleMusicSource : SourceBase
    {
        public override string Prefix { get; }

        public AppleMusicSource(Configuration config) : base(config)
        {
            Prefix = "apmsearch";
        }

        public override ValueTask<Stream> GetStreamAsync(string id)
        {
            throw new NotImplementedException();
        }

        public override ValueTask<SearchResult> SearchAsync(string query)
        {
            throw new NotImplementedException();
        }
    }
}
