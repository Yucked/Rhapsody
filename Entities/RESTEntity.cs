using System.Collections.Generic;
using System.Text.Json.Serialization;
using Frostbyte.Entities.Audio;
using Frostbyte.Entities.Enums;

namespace Frostbyte.Entities
{
    public sealed class RESTEntity
    {
        public RESTEntity(LoadType loadType = LoadType.NoMatches, IList<IAudioItem> audioItems = default)
        {
            LoadType = loadType;
            audioItems = audioItems;
        }

        [JsonPropertyName("l_type")]
        public LoadType LoadType { get; set; }

        [JsonPropertyName("tracks")]
        public IList<IAudioItem> audioItems { get; set; }
    }
}