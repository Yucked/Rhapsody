using System.Collections.Generic;
using System.Linq;
using Frostbyte.Entities.Audio;
using Frostbyte.Entities.Enums;

namespace Frostbyte.Entities.Results
{
    public sealed class SearchResult
    {
        public LoadType LoadType { get; set; }

        public AudioPlaylist Playlist { get; set; }

        public IEnumerable<AudioTrack> Tracks { get; set; }

        public SearchResult()
        {

        }

        public SearchResult(LoadType loadType = LoadType.NoMatches, IEnumerable<AudioTrack> tracks = default)
        {
            LoadType = loadType;
            Tracks = tracks.ToList();
        }
    }
}