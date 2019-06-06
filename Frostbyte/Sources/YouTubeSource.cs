using System.IO;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Frostbyte.Attributes;
using Frostbyte.Entities;
using Frostbyte.Entities.Audio;
using Frostbyte.Entities.Enums;
using Frostbyte.Entities.Results;
using Frostbyte.Extensions;
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

        public async ValueTask<RESTEntity> SearchAsync(string query)
        {
            var queryUrl = $"https://www.youtube.com/search_ajax?style=json&search_query={WebUtility.UrlEncode(query)}";
            var bytes = await HttpHandler.Instance.GetBytesAsync(queryUrl).ConfigureAwait(false);
            var result = JsonSerializer.Parse<YouTubeResult>(bytes.Span);
            var tracks = result.Video.Select(x => x.ToTrack).ToArray();
            return new RESTEntity(tracks.Length == 0 ? LoadType.NoMatches : LoadType.SearchResult, tracks);
        }

        public async ValueTask<Track> GetTrackAsync(string id)
        {
            if (TrySearchCache(id, out var audioItem))
            {
                return audioItem.TryCast<Track>();
            }

            return default;
        }

        public async ValueTask<Stream> GetStreamAsync(Track track)
        {
            throw new System.NotImplementedException();
        }

        public async ValueTask<Stream> GetStreamAsync(string id)
        {
            throw new System.NotImplementedException();
        }
    }
}