using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace Frostbyte.Enums
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum LoadType
    {
        [EnumMember(Value = "TRACK_LOADED")]
        TrackLoaded,

        [EnumMember(Value = "PLAYLIST_LOADED")]
        PlaylistLoaded,

        [EnumMember(Value = "SEARCH_RESULT")]
        SearchResult,

        [EnumMember(Value = "NO_MATCHES")]
        NoMatches,

        [EnumMember(Value = "LOAD_FAILED")]
        LoadFailed
    }
}