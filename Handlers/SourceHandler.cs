using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Frostbyte.Attributes;
using Frostbyte.Entities;
using Frostbyte.Extensions;
using Frostbyte.Sources;
using Microsoft.Extensions.DependencyInjection;
using TagLib.Matroska;

namespace Frostbyte.Handlers
{
    [Service(ServiceLifetime.Singleton)]
    public sealed class SourceHandler
    {
        private readonly IEnumerable<ISource> _sources;
        public ConcurrentDictionary<string, TrackEntity> Tracks { get; }

        public SourceHandler(IServiceProvider provider)
        {
            _sources = provider.GetServices(typeof(ISource)).Cast<ISource>();
            Tracks = new ConcurrentDictionary<string, TrackEntity>();
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
                YouTubeSource yt    => await yt.PrepareResponseAsync(query).ConfigureAwait(false),
                SoundCloudSource sc => await sc.PrepareResponseAsync(query).ConfigureAwait(false),
                LocalSource lc      => await lc.PrepareResponseAsync(query).ConfigureAwait(false)
            };

            _ = Task.Run(() =>
            {
                var rest = response.AdditionObject.TryCast<RESTEntity>();
                foreach (var track in rest.Tracks)
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