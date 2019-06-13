using Frostbyte.Entities.Audio;
using Frostbyte.Extensions;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization.Formatters.Binary;

namespace Frostbyte.Handlers
{
    public sealed class CacheHandler
    {
        private readonly ConcurrentDictionary<string, byte[]> _cache;
        public CacheHandler()
        {
            _cache = new ConcurrentDictionary<string, byte[]>();
        }

        public void Add(AudioTrack track)
        {
            if (_cache.TryGetValue(track.Hash, out _))
                return;

            using var memStream = new MemoryStream();
            using var gzip = new GZipStream(memStream, CompressionMode.Compress, false);
            var biFormatter = new BinaryFormatter();
            biFormatter.Serialize(gzip, track);

            _cache.TryAdd(track.Hash, memStream.ToArray());
        }

        public void Add(IEnumerable<AudioTrack> tracks)
        {
            foreach (var track in tracks)
                Add(track);
        }

        public bool TryGetFromCache(string hash, out AudioTrack track)
        {
            if (!_cache.TryGetValue(hash, out var compressed))
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