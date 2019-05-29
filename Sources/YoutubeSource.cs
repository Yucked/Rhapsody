using System.IO;
using System.Linq;
using System.Net;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Frostbyte.Attributes;
using Frostbyte.Entities;
using Frostbyte.Entities.Results;
using Frostbyte.Enums;
using Frostbyte.Handlers;
using Microsoft.Extensions.DependencyInjection;

namespace Frostbyte.Sources
{
    [Service(ServiceLifetime.Singleton)]
    public sealed class YoutubeSource : BaseSource
    {
        private const string ID_REGEX = @"(?:youtube\.com\/(?:[^\/]+\/.+\/|(?:v|e(?:mbed)?)\/|.*[?&]v=)|youtu\.be\/)([^""&?\/ ]{11})";

        public override bool IsEnabled
        {
            get => ConfigHandler.Config.Sources.YouTube;
        }

        public override string Prefix
        {
            get => "ytsearch";
        }

        public override async ValueTask<RESTEntity> PrepareResponseAsync(string query)
        {
            var queryUrl = $"https://www.youtube.com/search_ajax?style=json&search_query={WebUtility.UrlEncode(query)}";
            var bytes = await HttpHandler.Instance.GetBytesAsync(queryUrl).ConfigureAwait(false);
            var result = JsonSerializer.Parse<YouTubeResult>(bytes.Span);
            var tracks = result.Video.Select(x => x.ToTrack).ToArray();
            return tracks.Any()
                       ? new RESTEntity
                       {
                           LoadType = LoadType.SearchResult,
                           Tracks = tracks
                       }
                       : RESTEntity.Empty;
        }

        public override async ValueTask<Stream> GetStreamAsync(TrackEntity track)
        {
            throw new System.NotImplementedException();
        }
    }
}