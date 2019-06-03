using Frostbyte.Attributes;
using Frostbyte.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace Frostbyte.Handlers
{
    [Service(ServiceLifetime.Singleton)]
    public sealed class RatelimitHandler
    {
        private readonly RatelimitPolicy _policy;

        public RatelimitHandler(ConfigEntity config)
        {
            _policy = config.RatelimitPolicy;
        }

        public void Initialize()
        {

        }
    }
}