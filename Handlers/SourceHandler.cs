using System;
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
        private readonly YoutubeSource _youtubeSource;

        public SourceHandler(YoutubeSource youtubeSource)
        {
            _youtubeSource = youtubeSource;
        }

        public async Task<RESTEntity> HandlerRequestAsync(string query)
        {
            var split = query.Split(':');
            var id = split[0].ToLower();
            var url = split[1];

            RESTEntity response = default;

            switch (id)
            {
                case "ytsearch":
                    response = await _youtubeSource.PrepareResponseAsync(url).ConfigureAwait(false);
                    break;

                case "scsearch":
                    break;
            }

            return response;
        }
    }
}