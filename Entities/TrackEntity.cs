using System;

namespace Frostbyte.Entities
{
    public sealed class TrackEntity
    {
        public string Id { get; set; }

        public string Title { get; set; }

        public string Author { get; set; }

        public bool IsStream { get; set; }

        public bool IsSeekable { get; set; }

        public string ThumbnailUrl { get; set; }

        public string Url { get; set; }

        public TimeSpan Position
        {
            get => new TimeSpan(TrackPosition);
            set => TrackPosition = value.Ticks;
        }

        private long TrackPosition { get; set; }

        public TimeSpan Length
            => TimeSpan.FromMilliseconds(TrackLength);

        private long TrackLength { get; set; }
    }
}