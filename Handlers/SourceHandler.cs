using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Frostbyte.Attributes;
using Frostbyte.Entities;
using Frostbyte.Extensions;
using Frostbyte.Sources;
using Microsoft.Extensions.DependencyInjection;

namespace Frostbyte.Handlers
{
    [Service(ServiceLifetime.Singleton)]
    public sealed class SourceHandler
    {
        private readonly IEnumerable<BaseSource> _sources;

        public SourceHandler(IServiceProvider provider)
        {
            _sources = provider.GetServices(typeof(BaseSource)).Cast<BaseSource>();
        }

        public async Task<ResponseEntity> HandlerRequestAsync(string url)
        {
            var split = url.Split(':');
            var prefix = split[0].ToLower();
            var query = split[1];

            var source = _sources.FirstOrDefault(x => x.Prefix == prefix);
            var response = new ResponseEntity(false, source.IsEnabled ? $"{prefix.GetSourceFromPrefix()} endpoint is disabled in config." : "Success");

            if (!source.IsEnabled)
                return response;

            response.AdditionObject = source switch
            {
                YoutubeSource yt    => await yt.PrepareResponseAsync(query).ConfigureAwait(false),
                SoundCloudSource sc => await sc.PrepareResponseAsync(query).ConfigureAwait(false),
                LocalSource lc      => await lc.PrepareResponseAsync(query).ConfigureAwait(false)
            };

            return response;
        }
    }
}