using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Frostbyte.Attributes;
using Frostbyte.Entities;
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

        public async Task<RESTEntity> HandlerRequestAsync(string url)
        {
            var split = url.Split(':');
            var prefix = split[0].ToLower();
            var query = split[1];
            var response = RESTEntity.Empty;

            foreach (var source in _sources)
            {
                switch (source)
                {
                    case YoutubeSource yt when yt.Prefix == prefix:
                        if (!yt.IsEnabled)
                            break;
                        response = await yt.PrepareResponseAsync(query).ConfigureAwait(false);
                        break;

                    case SoundCloudSource sc when sc.Prefix == prefix:
                        if (!sc.IsEnabled)
                            break;
                        response = await sc.PrepareResponseAsync(query).ConfigureAwait(false);
                        break;
                }
            }

            return response;
        }
    }
}