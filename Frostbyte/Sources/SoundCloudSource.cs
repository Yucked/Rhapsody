using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Frostbyte.Attributes;
using Frostbyte.Entities;
using Frostbyte.Entities.Audio;
using Frostbyte.Entities.Enums;
using Frostbyte.Entities.Results;
using Frostbyte.Extensions;
using Frostbyte.Handlers;
using Microsoft.Extensions.DependencyInjection;

namespace Frostbyte.Sources
{
    [Service(ServiceLifetime.Singleton, typeof(ISourceProvider))]
    public sealed class SoundCloudSource : ISearchProvider, IStreamProvider
    {
        private const string BASE_URL = "https://api.soundcloud.com";

        public bool IsEnabled { get; }

        public string Prefix => "scsearch";

        public SoundCloudSource(ConfigEntity config)
        {
            IsEnabled = config.Sources.EnableSoundCloud;
        }
        
        public async ValueTask<RESTEntity> SearchAsync(
            string query,
            CancellationToken token = default)
        {
            var response = new RESTEntity();
            if (query.IsMatch(Constants.PATTERN_URL_SOUNDCLOUD))
            {
                query = $"{BASE_URL}/resolve?url={query}&client_id={Constants.CLIENT_ID_SOUNDCLOUD}";
                var bytes = await HttpHandler.Instance.GetBytesAsync(query).ConfigureAwait(false);
                var result = JsonSerializer.Parse<SoundCloudTrack>(bytes.Span);
                response.AudioItems.Add(result.ToTrack);
                response.LoadType = LoadType.TrackLoaded;
            }
            else
            {
                query = $"{BASE_URL}/tracks?q={query}&client_id={Constants.CLIENT_ID_SOUNDCLOUD}";
                var bytes = await HttpHandler.Instance.GetBytesAsync(query).ConfigureAwait(false);
                var result = JsonSerializer.Parse<IList<SoundCloudTrack>>(bytes.Span);
                var tracks = result.Select(x => x.ToTrack).ToArray();
                response.AudioItems = tracks;
                response.LoadType = LoadType.SearchResult;
            }

            return response;
        }

        public ValueTask<Stream> GetStreamAsync(IAudioItem audioItem, CancellationToken token = default)
            => GetStreamAsync(audioItem.Id, token);

        public async ValueTask<Stream> GetStreamAsync(string id, CancellationToken token = default)
        {
            var bytes = await HttpHandler.Instance.GetBytesAsync($"{BASE_URL}/tracks/stream?client_id={Constants.CLIENT_ID_SOUNDCLOUD}", token)
                                         .ConfigureAwait(false);
            var read = JsonSerializer.Parse<SoundCloudDirectUrl>(bytes.Span);
            return await HttpHandler.Instance.GetStreamAsync(read.Url).ConfigureAwait(false);
        }

        private async Task FetchClientId()
        {
            var raw = await HttpHandler.Instance.GetStringAsync("https://soundcloud.com").ConfigureAwait(false);
            raw.IsMatch(Constants.PATTERN_SOUNDCLOUD_SCRIPT);
        }
    }
}