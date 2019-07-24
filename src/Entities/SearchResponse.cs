using System.Collections.Generic;
using Frostbyte.Entities.Enums;
using Frostbyte.Entities.Infos;

namespace Frostbyte.Entities
{
    public sealed class SearchResponse
    {
        public Status Status { get; set; }
        public LoadType LoadType { get; set; }
        public PlaylistInfo Playlist { get; set; }
        public HashSet<TrackInfo> Tracks { get; }

        public SearchResponse()
        {
            Status = Status.Ok;
            Playlist = default;
            LoadType = LoadType.NoMatches;
            Tracks = new HashSet<TrackInfo>();
        }

        public static SearchResponse WithError(string error)
            => new SearchResponse
            {
                LoadType = LoadType.SearchError,
                Status = Status.Error(error)
            };
    }

    public struct Status
    {
        public bool IsSuccess { get; set; }
        public string Reason { get; set; }

        public static Status Error(string reason)
            => new Status
            {
                IsSuccess = false,
                Reason = reason
            };

        public static Status Ok
            => new Status
            {
                IsSuccess = true
            };
    }
}