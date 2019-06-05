using System.IO;
using System.Linq;
using System.Net;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Frostbyte.Attributes;
using Frostbyte.Entities;
using Frostbyte.Entities.Audio;
using Frostbyte.Entities.Enums;
using Frostbyte.Entities.Results;
using Frostbyte.Handlers;
using Microsoft.Extensions.DependencyInjection;

namespace Frostbyte.Sources
{
    [Service(ServiceLifetime.Singleton, typeof(ISourceProvider))]
    public sealed class YouTubeSource : ISearchProvider, IStreamProvider
    {
        public string Prefix => "ytsearch";
        public bool IsEnabled { get; }

        public YouTubeSource(ConfigEntity config)
        {
            IsEnabled = config.Sources.EnableYouTube;
        }

        public async ValueTask<RESTEntity> SearchAsync(
            string query,
            CancellationToken token = default)
        {
            var queryUrl = $"https://www.youtube.com/search_ajax?style=json&search_query={WebUtility.UrlEncode(query)}";
            var bytes = await HttpHandler.Instance.GetBytesAsync(queryUrl).ConfigureAwait(false);
            var result = JsonSerializer.Parse<YouTubeResult>(bytes.Span);
            var tracks = result.Video.Select(x => x.ToTrack).ToArray();
            return new RESTEntity(tracks.Length == 0 ? LoadType.NoMatches : LoadType.SearchResult, tracks);
        }

        public async ValueTask<Stream> GetStreamAsync(
            IAudioItem audioItem,
            CancellationToken token = default)
        {
            throw new System.NotImplementedException();
        }

        public ValueTask<Stream> GetStreamAsync(
            string id,
            CancellationToken token = default)
        {
            throw new System.NotImplementedException();
        }
    }
}