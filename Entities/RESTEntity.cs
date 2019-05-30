using Frostbyte.Enums;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Frostbyte.Entities
{
    public sealed class RESTEntity
    {
        public RESTEntity(LoadType loadType = LoadType.NoMatches, IList<TrackEntity> tracks = default)
        {
            LoadType = loadType;
            Tracks = tracks;
        }

        [JsonPropertyName("l_type")]
        public LoadType LoadType { get; set; }

        [JsonPropertyName("tracks")]
        public IList<TrackEntity> Tracks { get; set; }
    }
}