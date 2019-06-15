namespace Frostbyte.Entities.Audio
{
    public sealed class AudioPlaylist
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string Url { get; set; }

        public long Duration { get; set; }

        public string ArtworkUrl { get; set; }
    }
}
