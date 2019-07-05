using System.Runtime.Serialization;

namespace Frostbyte.Entities.Enums
{
    public enum LoadType
    {
        [EnumMember(Value = "TRACK_LOADED")]
        TrackLoaded = 1,

        [EnumMember(Value = "PLAYLIST_LOADED")]
        PlaylistLoaded = 2,

        [EnumMember(Value = "SEARCH_RESULT")]
        SearchResult = 3,

        [EnumMember(Value = "NO_MATCHES")]
        NoMatches = 4,

        [EnumMember(Value = "LOAD_FAILED")]
        LoadFailed = 5
    }
}