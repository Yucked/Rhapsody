using System.Collections.Generic;
using Frostbyte.Entities.Audio;
using Frostbyte.Entities.Enums;

namespace Frostbyte.Entities.Responses
{
    public sealed class SearchResponse
    {
        public LoadType LoadType { get; set; }
        public AudioPlaylist Playlist { get; set; }
        public IEnumerable<AudioTrack> Tracks { get; set; }
    }
}