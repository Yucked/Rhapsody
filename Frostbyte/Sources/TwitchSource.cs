using System.IO;
using System.Threading.Tasks;

using Frostbyte.Entities;
using Frostbyte.Entities.Results;

namespace Frostbyte.Sources
{
    
    public sealed class TwitchSource : SourceBase
    {
        public override string Prefix { get; }

        public const string
            CLIENT_ID = "jzkbprff40iqj646a697cyrvl0zt2m6",
            ACCESS_TOKEN = "https://api.twitch.tv/api/channels/{0}/access_token?adblock=false&need_https=true&platform=web&player_type=site";

        public TwitchSource(Configuration config) : base(config)
        {
            Prefix = "twsearch";
        }

        public override ValueTask<SearchResult> SearchAsync(string query)
        {
            throw new System.NotImplementedException();
        }

        public override ValueTask<Stream> GetStreamAsync(string id)
        {
            throw new System.NotImplementedException();
        }
    }
}