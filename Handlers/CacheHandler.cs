using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization.Formatters.Binary;
using Frostbyte.Entities.Audio;
using Frostbyte.Extensions;

namespace Frostbyte.Handlers
{
    public sealed class CacheHandler
    {
        private readonly ConcurrentDictionary<string, byte[]> _cache;

        public CacheHandler()
        {
            _cache = new ConcurrentDictionary<string, byte[]>();
        }

        private void Add(AudioTrack track)
        {
            if (_cache.TryGetValue(track.Id, out _))
                return;

            using var memStream = new MemoryStream();
            using var gzip = new GZipStream(memStream, CompressionMode.Compress, false);
            var biFormatter = new BinaryFormatter();
            biFormatter.Serialize(gzip, track);

            _cache.TryAdd(track.Id, memStream.ToArray());
        }

        public void Add(IEnumerable<AudioTrack> tracks)
        {
            foreach (var track in tracks)
                Add(track);
        }

        public bool TryGetFromCache(string id, out AudioTrack track)
        {
            if (!_cache.TryGetValue(id, out var compressed))
            {
                track = default;
                return false;
            }

            using var memStream = new MemoryStream(compressed);
            using var gzip = new GZipStream(memStream, CompressionMode.Decompress, false);
            var biFormatter = new BinaryFormatter();
            track = biFormatter.Deserialize(gzip).TryCast<AudioTrack>();
            return true;
        }

        public void Clear()
        {
            _cache.Clear();
        }
    }
}