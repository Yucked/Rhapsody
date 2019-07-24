using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Frostbyte.Entities.Infos;

namespace Frostbyte.Entities.Results
{
    public sealed class BandCampResult
    {
        [JsonPropertyName("current")]
        public BandCampCurrent Current { get; set; }

        [JsonPropertyName("art_id")]
        public long ArtId { get; set; }

        [JsonPropertyName("trackinfo")]
        public IEnumerable<BandCampTrack> Trackinfo { get; set; }

        [JsonPropertyName("url")]
        public string Url { get; set; }

        [JsonPropertyName("artist")]
        public string Artist { get; set; }

        [JsonPropertyName("item_type")]
        public string ItemType { get; set; }
    }

    public class BandCampCurrent
    {
        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("streaming")]
        public long Streaming { get; set; }

        [JsonPropertyName("id")]
        public long Id { get; set; }
    }

    public sealed class BandCampTrack
    {
        [JsonPropertyName("streaming")]
        public int Streaming { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("track_id")]
        public long TrackId { get; set; }

        [JsonPropertyName("file")]
        public BandCampFile File { get; set; }

        [JsonPropertyName("duration")]
        public double Duration { get; set; }

        public TrackInfo ToTrackInfo(string author, string url, long artId)
            => new TrackInfo($"{TrackId}",
                Title,
                url,
                (long) TimeSpan.FromSeconds(Duration)
                    .TotalMilliseconds,
                artId == 0
                    ? default
                    : $"https://f4.bcbits.com/img/a{artId}_0.jpg",
                Streaming == 1,
                "BandCamp",
                new AuthorInfo(author, default, default));
    }

    public struct BandCampFile
    {
        [JsonPropertyName("mp3-128")]
        public string Mp3Url { get; set; }
    }
}