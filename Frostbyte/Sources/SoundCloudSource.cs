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
    [Service(ServiceLifetime.Singleton)]
    public sealed class SoundCloudSource : ISearchProvider, IStreamProvider
    {
        private const string BASE_URL = "https://api.soundcloud.com",
                             CLIENT_ID = "a3dd183a357fcff9a6943c0d65664087",
                             REGEX_PATTERN = @"/^https?:\/\/(soundcloud\.com|snd\.sc)\/(.*)$/";

        public bool IsEnabled => ConfigHandler.Config.Sources.Soundcloud;

        public string Prefix => "scsearch";
        
        public async ValueTask<RESTEntity> SearchAsync(string query, CancellationToken cancellationToken = default)
        {
            var response = new RESTEntity();
            if (REGEX_PATTERN.ToRegex().Match(query).Success)
            {
                query = $"{BASE_URL}/resolve?url={query}&client_id={CLIENT_ID}";
                var bytes = await HttpHandler.Instance.GetBytesAsync(query, cancellationToken).ConfigureAwait(false);
                var result = JsonSerializer.Parse<SoundCloudTrack>(bytes.Span);
                response.AudioItems.Add(result.ToTrack);
                response.LoadType = LoadType.TrackLoaded;
            }
            else
            {
                query = $"{BASE_URL}/tracks?q={query}&client_id={CLIENT_ID}";
                var bytes = await HttpHandler.Instance.GetBytesAsync(query, cancellationToken).ConfigureAwait(false);
                var result = JsonSerializer.Parse<IList<SoundCloudTrack>>(bytes.Span);
                var tracks = result.Select(x => x.ToTrack).ToArray();
                response.AudioItems = tracks;
                response.LoadType = LoadType.SearchResult;
            }

            return response;
        }

        public ValueTask<Stream> GetStreamAsync(IAudioItem audioItem, CancellationToken cancellationToken = default)
        {
            throw new System.NotImplementedException();
        }
    }
}