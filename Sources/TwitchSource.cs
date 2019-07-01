using System.IO;
using System.Threading.Tasks;
using Frostbyte.Entities.Responses;

namespace Frostbyte.Sources
{
    public sealed class TwitchSource : BaseSourceProvider
    {
        public const string
            CLIENT_ID = "jzkbprff40iqj646a697cyrvl0zt2m6",
            ACCESS_TOKEN = "https://api.twitch.tv/api/channels/{0}/access_token?adblock=false&need_https=true&platform=web&player_type=site";

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