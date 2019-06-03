using Frostbyte.Entities;
using System.IO;
using System.Threading.Tasks;

namespace Frostbyte.Sources
{
    public interface ISource
    {
        public const string NO_EMBED_URL
            = "http://noembed.com/embed?url={0}&callback=my_embed_function";

        string Prefix { get; }

        bool IsEnabled { get; }

        ValueTask<RESTEntity> PrepareResponseAsync(string query);

        ValueTask<Stream> GetStreamAsync(string id);

        ValueTask<Stream> GetStreamAsync(TrackEntity track);
    }
}