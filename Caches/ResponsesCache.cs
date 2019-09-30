using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Concept.Options;
using Microsoft.Extensions.Options;

namespace Concept.Caches
{
    public sealed class ResponsesCache
    {
        private readonly CacheOptions _cacheOptions;
        private readonly ConcurrentDictionary<object, object> _responses;

        public ResponsesCache(IOptions<ApplicationOptions> aOptions)
        {
            _cacheOptions = aOptions.Value.CacheOptions;
            _responses = new ConcurrentDictionary<object, object>();
        }

        public async Task AutoPurgeAsync()
        {
            while (_cacheOptions.IsEnabled)
            {
                await Task.Delay(TimeSpan.FromMinutes(10));
            }
        }
    }
}