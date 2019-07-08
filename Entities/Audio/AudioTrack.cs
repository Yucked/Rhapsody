namespace Frostbyte.Entities.Audio
{
    public sealed class AudioTrack
    {
        public string Id { get; set; }
        public string Url { get; set; }
        public string Title { get; set; }
        public long Duration { get; set; }
        public long Position { get; set; }
        public string Provider { get; set; }
        public bool CanStream { get; set; }
        public string ArtworkUrl { get; set; }
        public TrackAuthor Author { get; set; }
    }
}