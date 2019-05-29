using Frostbyte.Enums;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Frostbyte.Entities
{
    public sealed class RESTEntity
    {
        [JsonPropertyName("l_type")]
        public LoadType LoadType { get; set; }

        [JsonPropertyName("tracks")]
        public IList<TrackEntity> Tracks { get; set; }

        public static RESTEntity Empty
            => new RESTEntity
            {
                LoadType = LoadType.NoMatches,
                Tracks = default
            };
    }
}