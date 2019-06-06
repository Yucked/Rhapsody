using System;
using System.Text.Json.Serialization;

namespace Frostbyte.Entities.Audio
{
    public sealed class Track : IAudioItem
    {
        public string Id { get; set; }
        public string Url { get; set; }
        public string Title { get; set; }
        public Author Author { get; set; }
        public string ThumbnailUrl { get; set; }

        [JsonIgnore]
        public TimeSpan Position
        {
            get => new TimeSpan(TrackPosition);
            set => TrackPosition = value.Ticks;
        }

        private long TrackPosition { get; set; }

        [JsonIgnore]
        public TimeSpan Length
        {
            get => TimeSpan.FromMilliseconds(TrackLength);
        }

        public long TrackLength { get; set; }
    }
}