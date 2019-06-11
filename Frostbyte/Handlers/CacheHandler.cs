using Frostbyte.Entities.Audio;
using Frostbyte.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;

namespace Frostbyte.Handlers
{
    public sealed class CacheHandler
    {
        private readonly HashSet<(string Id, string Title, string Url, byte[] Compressed)> _cache;
        public CacheHandler()
        {
            _cache = new HashSet<(string Id, string Title, string Url, byte[] Compressed)>();
        }

        public void Add(AudioTrack track)
        {
            if (_cache.Any(x =>
            x.Id.Equals(track.Id, StringComparison.InvariantCultureIgnoreCase) ||
            x.Title.Equals(track.Title, StringComparison.InvariantCultureIgnoreCase) ||
            x.Url.Equals(track.Url, StringComparison.InvariantCultureIgnoreCase)))
                return;

            using var memStream = new MemoryStream();
            using var gzip = new GZipStream(memStream, CompressionMode.Compress, false);
            var biFormatter = new BinaryFormatter();
            biFormatter.Serialize(gzip, track);
            var array = memStream.ToArray();

            _cache.Add((track.Id, track.Title, track.Url, array));
        }

        public void Add(IEnumerable<AudioTrack> tracks)
        {
            foreach (var track in tracks)
                Add(track);
        }

        public bool ExistsInCache(string query, out AudioTrack track)
        {
            var cache = _cache.FirstOrDefault(x =>
            x.Id.Equals(query, StringComparison.InvariantCultureIgnoreCase) ||
            x.Title.Equals(query, StringComparison.InvariantCultureIgnoreCase) ||
            x.Url.Equals(query, StringComparison.InvariantCultureIgnoreCase));

            if (cache.Compressed.Length == 0)
            {
                track = default;
                return false;
            }

            using var memStream = new MemoryStream(cache.Compressed);
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