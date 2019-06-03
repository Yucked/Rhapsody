using System.IO;
using System.Linq;
using System.Net;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Frostbyte.Attributes;
using Frostbyte.Entities;
using Frostbyte.Entities.Enums;
using Frostbyte.Entities.Results;
using Frostbyte.Handlers;
using Microsoft.Extensions.DependencyInjection;

namespace Frostbyte.Sources
{
    [Service(ServiceLifetime.Singleton, typeof(ISource))]
    public sealed class YouTubeSource : ISource
    {
        public string Prefix { get; }
        public bool IsEnabled { get; }
        private const string ID_REGEX
            = @"(?:youtube\.com\/(?:[^\/]+\/.+\/|(?:v|e(?:mbed)?)\/|.*[?&]v=)|youtu\.be\/)([^""&?\/ ]{11})";

        public YouTubeSource(ConfigEntity config)
        {
            Prefix = "ytsearch";
            IsEnabled = config.Sources.EnableYouTube;
        }

        public async ValueTask<RESTEntity> PrepareResponseAsync(string query)
        {
            var queryUrl = $"https://www.youtube.com/search_ajax?style=json&search_query={WebUtility.UrlEncode(query)}";
            var bytes = await HttpHandler.Instance.GetBytesAsync(queryUrl).ConfigureAwait(false);
            var result = JsonSerializer.Parse<YouTubeResult>(bytes.Span);
            var tracks = result.Video.Select(x => x.ToTrack).ToArray();
            return new RESTEntity(tracks.Length == 0 ? LoadType.NoMatches : LoadType.SearchResult, tracks);
        }

        public async ValueTask<Stream> GetStreamAsync(TrackEntity track)
        {
            throw new System.NotImplementedException();
        }

        public async ValueTask<Stream> GetStreamAsync(string id)
        {
            throw new System.NotImplementedException();
        }
    }
}