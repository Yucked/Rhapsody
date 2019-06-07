using System.IO;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Frostbyte.Attributes;
using Frostbyte.Entities;
using Frostbyte.Entities.Enums;
using Frostbyte.Entities.Results;
using Frostbyte.Handlers;

namespace Frostbyte.Sources
{
    [RegisterService(typeof(ISourceProvider))]
    public sealed class YouTubeSource : SourceCache, ISourceProvider
    {
        public string Prefix { get; }
        public bool IsEnabled { get; }

        public YouTubeSource(Configuration config)
        {
            Prefix = "ytsearch";
            IsEnabled = config.Sources.EnableYouTube;
        }

        public async ValueTask<SearchResult> SearchAsync(string query)
        {
            var get = await HttpHandler.Instance
                .WithUrl("https://www.youtube.com/search_ajax")
                .WithParameter("style", "json")
                .WithParameter("search_query", WebUtility.UrlEncode(query))
                .GetBytesAsync().ConfigureAwait(false);

            var result = JsonSerializer.Parse<YouTubeResult>(get.Span);
            var tracks = result.Video.Select(x => x.ToTrack).ToArray();
            return new SearchResult(tracks.Length == 0 ? LoadType.NoMatches : LoadType.SearchResult, tracks);
        }

        public async ValueTask<Stream> GetStreamAsync(string id)
        {
            throw new System.NotImplementedException();
        }
    }
}