using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Frostbyte.Attributes;
using Frostbyte.Entities;
using Frostbyte.Entities.Audio;
using Frostbyte.Extensions;
using Frostbyte.Sources;
using Microsoft.Extensions.DependencyInjection;

namespace Frostbyte.Handlers
{
    [Service(ServiceLifetime.Singleton)]
    public sealed class SourceHandler
    {
        private readonly IEnumerable<ISourceProvider> _sources;
        public ConcurrentDictionary<string, Track> Tracks { get; }

        public SourceHandler(IServiceProvider provider)
        {
            _sources = provider.GetServices<ISourceProvider>();
            Tracks = new ConcurrentDictionary<string, Track>();
        }

        public async Task<ResponseEntity> HandlerRequestAsync(string url)
        {
            var split = url.Split(':');
            var prefix = split[0].ToLower();
            var query = split[1];

            var source = _sources.FirstOrDefault(x => x.Prefix == prefix);
            var response = new ResponseEntity(false, !source.IsEnabled ? $"{prefix.GetSourceFromPrefix()} endpoint is disabled in config" : "Success");

            if (!source.IsEnabled)
                return response;

            response.IsSuccess = true;
            response.AdditionObject = source switch
            {
                ISearchProvider searchProvider => await searchProvider.SearchAsync(query)
                    .ConfigureAwait(false),
//                ITrackProvider trackProvider => await trackProvider.GetTrackAsync(query)
//                    .ConfigureAwait(false),
//                IPlaylistProvider playlistProvider => await playlistProvider.GetPlaylistAsync(query)
//                    .ConfigureAwait(false)
            };

            _ = Task.Run(() =>
            {
                var rest = response.AdditionObject.TryCast<RESTEntity>();
                foreach (var track in rest.AudioItems.Where(iAudioItem => iAudioItem is Track).Cast<Track>())
                {
                    if (Tracks.ContainsKey(track.Id))
                        continue;

                    Tracks.TryAdd(track.Id, track);
                }
            }).ConfigureAwait(false);

            return response;
        }
    }
}