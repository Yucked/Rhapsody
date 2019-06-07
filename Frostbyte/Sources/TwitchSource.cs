using System.IO;
using System.Threading.Tasks;
using Frostbyte.Attributes;
using Frostbyte.Entities;
using Frostbyte.Entities.Results;

namespace Frostbyte.Sources
{
    [RegisterService(typeof(ISourceProvider))]
    public sealed class TwitchSource : ISourceProvider
    {
        public string Prefix { get; }
        public bool IsEnabled { get; }

        public const string
            CLIENT_ID = "jzkbprff40iqj646a697cyrvl0zt2m6",
            ACCESS_TOKEN = "https://api.twitch.tv/api/channels/{0}/access_token?adblock=false&need_https=true&platform=web&player_type=site";

        public TwitchSource(Configuration config)
        {
            Prefix = "twsearch";
            IsEnabled = config.Sources.EnableTwitch;
        }

        public ValueTask<SearchResult> SearchAsync(string query)
        {
            throw new System.NotImplementedException();
        }

        public ValueTask<Stream> GetStreamAsync(string id)
        {
            throw new System.NotImplementedException();
        }
    }
}