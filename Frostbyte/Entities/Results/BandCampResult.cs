using Frostbyte.Entities.Audio;
using Frostbyte.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace Frostbyte.Entities.Results
{
    public sealed class BandCampResult
    {
        [JsonPropertyName("current")]
        public BandCampCurrent Current { get; set; }

        [JsonPropertyName("art_id")]
        public long ArtId { get; set; }

        [JsonPropertyName("trackinfo")]
        public List<BandCampTrack> Trackinfo { get; set; }

        [JsonPropertyName("url")]
        public string Url { get; set; }

        [JsonPropertyName("artist")]
        public string Artist { get; set; }

        [JsonPropertyName("item_type")]
        public string ItemType { get; set; }

        [JsonIgnore]
        public IEnumerable<AudioTrack> Tracks
            => Trackinfo.Select(x => x.ToAudioTrack(Artist, Url, ArtId));

        public AudioPlaylist ToAudioPlaylist
            => ItemType == "album"
            ? new AudioPlaylist
            {
                Id = $"{Current.Id}",
                Name = Current.Title,
                Duration = Tracks.Sum(x => x.Duration),
                ArtworkUrl = ArtId == 0 ? default : $"https://f4.bcbits.com/img/a{ArtId}_0.jpg"
            } : default;
    }

    public partial class BandCampCurrent
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

        public AudioTrack ToAudioTrack(string author, string url, long artId)
            => new AudioTrack
            {
                Id = $"{TrackId}",
                Title = Title,
                Duration = TimeSpan.FromSeconds(Duration).TotalMilliseconds.TryCast<int>(),
                CanStream = Streaming == 1,
                Author = new TrackAuthor
                {
                    Name = author
                },
                Url = url,
                ArtworkUrl = artId == 0 ? default : $"https://f4.bcbits.com/img/a{artId}_0.jpg"
            };
    }

    public struct BandCampFile
    {
        [JsonPropertyName("mp3-128")]
        public string Mp3Url { get; set; }
    }
}