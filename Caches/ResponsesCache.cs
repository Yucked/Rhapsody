using System;
using System.Collections.Concurrent;
using System.Linq;
using Concept.Entities.Options;
using Microsoft.Extensions.Options;
using Theory.Providers;
using Theory.Search;

namespace Concept.Caches
{
    public sealed class ResponsesCache
    {
        private readonly CacheOptions _cacheOptions;
        public ConcurrentDictionary<DateTimeOffset, SearchResponse> YtCache { get; }
        public ConcurrentDictionary<DateTimeOffset, SearchResponse> ScCache { get; }
        public ConcurrentDictionary<DateTimeOffset, SearchResponse> BcCache { get; }

        public ResponsesCache(IOptions<ApplicationOptions> aOptions)
        {
            _cacheOptions = aOptions.Value.CacheOptions;
            YtCache = new ConcurrentDictionary<DateTimeOffset, SearchResponse>();
            ScCache = new ConcurrentDictionary<DateTimeOffset, SearchResponse>();
            BcCache = new ConcurrentDictionary<DateTimeOffset, SearchResponse>();
        }

        public void RemoveExpiredEntries(ConcurrentDictionary<DateTimeOffset, SearchResponse> cache)
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
                ProviderType.YouTube    => TryGetAnyCache(YtCache, query, out value),
                ProviderType.SoundCloud => TryGetAnyCache(ScCache, query, out value),
                ProviderType.BandCamp   => TryGetAnyCache(BcCache, query, out value),
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
                ProviderType.YouTube    => TryAddAnyCache(YtCache, value),
                ProviderType.SoundCloud => TryAddAnyCache(ScCache, value),
                ProviderType.BandCamp   => TryAddAnyCache(BcCache, value),
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