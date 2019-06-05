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
    [Service(ServiceLifetime.Singleton)]
    public sealed class YouTubeSource : ISearchProvider, IStreamProvider
    {
        private const string ID_REGEX = @"(?:youtube\.com\/(?:[^\/]+\/.+\/|(?:v|e(?:mbed)?)\/|.*[?&]v=)|youtu\.be\/)([^""&?\/ ]{11})";

        public bool IsEnabled => ConfigHandler.Config.Sources.YouTube;

        public string Prefix => "ytsearch";

        public async ValueTask<RESTEntity> SearchAsync(
            string query,
            CancellationToken cancellationToken = default)
        {
            var queryUrl = $"https://www.youtube.com/search_ajax?style=json&search_query={WebUtility.UrlEncode(query)}";
            var bytes = await HttpHandler.Instance.GetBytesAsync(queryUrl, cancellationToken).ConfigureAwait(false);
            var result = JsonSerializer.Parse<YouTubeResult>(bytes.Span);
            IAudioItem[] tracks = result.Video.Select(x => x.ToTrack).ToArray();
            return new RESTEntity(tracks.Length == 0 ? LoadType.NoMatches : LoadType.SearchResult, tracks);
        }

        public ValueTask<Stream> GetStreamAsync(
            IAudioItem audioItem,
            CancellationToken cancellationToken = default)
        {
            throw new System.NotImplementedException();
        }
    }
}