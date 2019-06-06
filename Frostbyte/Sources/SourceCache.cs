using Frostbyte.Entities.Audio;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Frostbyte.Sources
{
    public class SourceCache
    {
        private readonly ConcurrentDictionary<string, IAudioItem> _audioCache
            = new ConcurrentDictionary<string, IAudioItem>();

        public void AddToCache(IEnumerable<IAudioItem> audioItems)
        {
            foreach (var item in audioItems)
            {
                if (_audioCache.ContainsKey(item.Id))
                    continue;

                _audioCache.TryAdd(item.Id, item);
            }
        }

        public bool TrySearchCache(string query, out IAudioItem audioItem)
        {
            return _audioCache.TryGetValue(query, out audioItem);
        }
    }
}