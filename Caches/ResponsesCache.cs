using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Concept.Entities.Options;
using Microsoft.Extensions.Options;
using Theory.Providers;
using Theory.Search;

namespace Concept.Caches
{
    public sealed class ResponsesCache
    {
        private readonly CacheOptions _cacheOptions;
        private readonly ConcurrentDictionary<DateTimeOffset, SearchResponse> _ytCache;
        private readonly ConcurrentDictionary<DateTimeOffset, SearchResponse> _scCache;
        private readonly ConcurrentDictionary<DateTimeOffset, SearchResponse> _bcCache;

        public ResponsesCache(IOptions<ApplicationOptions> aOptions)
        {
            _cacheOptions = aOptions.Value.CacheOptions;
            _ytCache = new ConcurrentDictionary<DateTimeOffset, SearchResponse>();
            _scCache = new ConcurrentDictionary<DateTimeOffset, SearchResponse>();
            _bcCache = new ConcurrentDictionary<DateTimeOffset, SearchResponse>();
        }

        public async Task AutoPurgeAsync()
        {
            while (_cacheOptions.IsEnabled && _cacheOptions.ExpiresAfter > 0)
            {
                RemoveExpiredEntries(_ytCache);
                RemoveExpiredEntries(_scCache);
                RemoveExpiredEntries(_bcCache);
                await Task.Delay(TimeSpan.FromMinutes(10));
            }
        }

        private void RemoveExpiredEntries(ConcurrentDictionary<DateTimeOffset, SearchResponse> cache)
        {
            foreach (var (key, _) in cache)
            {
                if (key < DateTimeOffset.Now)
                    continue;

                cache.TryRemove(key, out _);
            }
        }

        public bool TryGetCache(string query, out SearchResponse value, ProviderType provider)
        {
            return provider switch
            {
                ProviderType.YouTube    => TryGetAnyCache(_ytCache, query, out value),
                ProviderType.SoundCloud => TryGetAnyCache(_scCache, query, out value),
                ProviderType.BandCamp   => TryGetAnyCache(_bcCache, query, out value),
                _                       => throw new Exception($"Invalid provider {provider}.")
            };
        }

        private bool TryGetAnyCache(
            ConcurrentDictionary<DateTimeOffset, SearchResponse> cache,
            string query,
            out SearchResponse value)
        {
            foreach (var cacheResponse in cache.Values)
            {
                if (query.Contains(cacheResponse.Query))
                {
                    value = cacheResponse;
                    return true;
                }

                if (!cacheResponse.Tracks.Any(track => track.Id == query || track.Url == query))
                    continue;
                
                value = cacheResponse;
                return true;
            }

            value = default;
            return false;
        }

        public bool TryAddCache(SearchResponse value, ProviderType provider)
        {
            return provider switch
            {
                ProviderType.YouTube    => TryAddAnyCache(_ytCache, value),
                ProviderType.SoundCloud => TryAddAnyCache(_scCache, value),
                ProviderType.BandCamp   => TryAddAnyCache(_bcCache, value),
                _                       => throw new Exception($"Invalid provider {provider}.")
            };
        }

        private bool TryAddAnyCache(
            ConcurrentDictionary<DateTimeOffset, SearchResponse> cache,
            SearchResponse value)
        {
            if (_cacheOptions.Limit <= 0 || cache.Count < _cacheOptions.Limit)
                return cache.TryAdd(DateTimeOffset.Now.AddMinutes(_cacheOptions.ExpiresAfter), value);

            cache.TryRemove(cache.FirstOrDefault()
                .Key, out _);
            return cache.TryAdd(DateTimeOffset.Now.AddMinutes(_cacheOptions.ExpiresAfter), value);
        }
    }
}