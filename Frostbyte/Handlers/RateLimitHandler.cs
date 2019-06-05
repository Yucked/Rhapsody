using Frostbyte.Attributes;
using Frostbyte.Entities;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;
using System.Net;

namespace Frostbyte.Handlers
{
    [Service(ServiceLifetime.Singleton)]
    public sealed class RatelimitHandler
    {
        private readonly RatelimitPolicy _policy;
        private readonly ConcurrentDictionary<IPEndPoint, RatelimitBucket> _buckets;

        public RatelimitHandler(ConfigEntity config)
        {
            _policy = config.RatelimitPolicy;
            _buckets = new ConcurrentDictionary<IPEndPoint, RatelimitBucket>();
        }

        public void Add(IPEndPoint endpoint)
        {
            
        }

        public bool ShouldPass(IPEndPoint endPoint)
        {
            return false;
        }
        
        
    }
}