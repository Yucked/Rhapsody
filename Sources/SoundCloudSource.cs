using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Frostbyte.Attributes;
using Frostbyte.Entities;
using Frostbyte.Entities.Enums;
using Frostbyte.Entities.Results;
using Frostbyte.Extensions;
using Frostbyte.Handlers;
using Microsoft.Extensions.DependencyInjection;

namespace Frostbyte.Sources
{
    [Service(ServiceLifetime.Singleton, typeof(ISource))]
    public sealed class SoundCloudSource : ISource
    {
        public string Prefix { get; }
        public bool IsEnabled { get; }
        private const string BASE_URL = "https://api.soundcloud.com",
                     CLIENT_ID = "client_id=a3dd183a357fcff9a6943c0d65664087",
                     REGEX_PATTERN = @"/^https?:\/\/(soundcloud\.com|snd\.sc)\/(.*)$/";

        public SoundCloudSource(ConfigEntity config)
        {
            Prefix = "scsearch";
            IsEnabled = config.Sources.EnableSoundCloud;
        }

        public async ValueTask<RESTEntity> PrepareResponseAsync(string query)
        {
            var response = new RESTEntity();
            if (query.IsMatch(REGEX_PATTERN))
            {
                query = $"{BASE_URL}/resolve?url={query}&{CLIENT_ID}";
                var bytes = await HttpHandler.Instance.GetBytesAsync(query).ConfigureAwait(false);
                var result = JsonSerializer.Parse<SoundCloudTrack>(bytes.Span);
                response.Tracks.Add(result.ToTrack);
                response.LoadType = LoadType.TrackLoaded;
            }
            else
            {
                query = $"{BASE_URL}/tracks?q={query}&{CLIENT_ID}";
                var bytes = await HttpHandler.Instance.GetBytesAsync(query).ConfigureAwait(false);
                var result = JsonSerializer.Parse<IList<SoundCloudTrack>>(bytes.Span);
                var tracks = result.Select(x => x.ToTrack).ToArray();
                response.Tracks = tracks;
                response.LoadType = LoadType.SearchResult;
            }

            return response;
        }

        public ValueTask<Stream> GetStreamAsync(TrackEntity track)
        {
            return GetStreamAsync(track.Id);
        }

        public async ValueTask<Stream> GetStreamAsync(string id)
        {
            var bytes = await HttpHandler.Instance.GetBytesAsync($"{BASE_URL}/tracks/stream?{CLIENT_ID}").ConfigureAwait(false);
            var read = JsonSerializer.Parse<SoundCloudDirectUrl>(bytes.Span);
            var stream = await HttpHandler.Instance.GetStreamAsync(read.Url).ConfigureAwait(false);
            return stream;
        }
    }
}