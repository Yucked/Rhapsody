using System.Collections.Generic;
using System.IO;
using System.Linq;
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
    public sealed class SoundCloudSource : SourceCache, ISourceProvider
    {
        public string Prefix { get; }
        public bool IsEnabled { get; }
        private const string BASE_URL = "https://api.soundcloud.com";

        public SoundCloudSource(Configuration config)
        {
            Prefix = "scsearch";
            IsEnabled = config.Sources.EnableSoundCloud;
        }

        public async ValueTask<RESTEntity> SearchAsync(string query)
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

            AddToCache(response.AudioItems);
            return response;
        }

        public ValueTask<Track> GetTrackAsync(string id)
        {
            throw new System.NotImplementedException();
        }

        public ValueTask<Stream> GetStreamAsync(Track track)
        {
            return GetStreamAsync(track.Id);
        }

        public async ValueTask<Stream> GetStreamAsync(string id)
        {
            var bytes = await HttpHandler.Instance.GetBytesAsync($"{BASE_URL}/tracks/{id}/stream?client_id={Constants.CLIENT_ID_SOUNDCLOUD}")
                                         .ConfigureAwait(false);
            var read = JsonSerializer.Parse<SoundCloudDirectUrl>(bytes.Span);
            var stream = await HttpHandler.Instance.GetStreamAsync(read.Url).ConfigureAwait(false);
            return stream;
        }

        private async Task FetchClientId()
        {
            var raw = await HttpHandler.Instance.GetStringAsync("https://soundcloud.com").ConfigureAwait(false);
            raw.IsMatch(Constants.PATTERN_SOUNDCLOUD_SCRIPT);
        }
    }
}