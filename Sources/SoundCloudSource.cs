using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json.Serialization;
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
    [Service(ServiceLifetime.Singleton, typeof(BaseSource))]
    public sealed class SoundCloudSource : BaseSource
    {
        private const string BASE_URL = "https://api.soundcloud.com",
                             CLIENT_ID = "a3dd183a357fcff9a6943c0d65664087",
                             REGEX_PATTERN = @"/^https?:\/\/(soundcloud\.com|snd\.sc)\/(.*)$/";

        public override bool IsEnabled
        {
            get => ConfigHandler.Config.Sources.Soundcloud;
        }

        public override string Prefix
        {
            get => "scsearch";
        }

        public override async ValueTask<RESTEntity> PrepareResponseAsync(string query)
        {
            var response = new RESTEntity();
            if (Regex(REGEX_PATTERN).Match(query).Success)
            {
                query = $"{BASE_URL}/resolve?url={query}&client_id={CLIENT_ID}";
                var bytes = await HttpHandler.Instance.GetBytesAsync(query).ConfigureAwait(false);
                var result = JsonSerializer.Parse<SoundCloudTrack>(bytes.Span);
                response.audioItems.Add(result.ToTrack);
                response.LoadType = LoadType.TrackLoaded;
            }
            else
            {
                query = $"{BASE_URL}/tracks?q={query}&client_id={CLIENT_ID}";
                var bytes = await HttpHandler.Instance.GetBytesAsync(query).ConfigureAwait(false);
                var result = JsonSerializer.Parse<IList<SoundCloudTrack>>(bytes.Span);
                var tracks = result.Select(x => x.ToTrack).ToArray();
                response.audioItems = tracks;
                response.LoadType = LoadType.SearchResult;
            }

            return response;
        }

        public override async ValueTask<Stream> GetStreamAsync(Track track)
        {
            throw new System.NotImplementedException();
        }
    }
}