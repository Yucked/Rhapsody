namespace Frostbyte.Entities.Infos
{
    public readonly struct PlaylistInfo
    {
        public string Id { get; }
        public string Name { get; }
        public string Url { get; }
        public long Duration { get; }
        public string ArtworkUrl { get; }

        public PlaylistInfo(string id, string name, string url, long duration, string artworkUrl)
        {
            Id = id;
            Name = name;
            Url = url;
            Duration = duration;
            ArtworkUrl = artworkUrl;
        }
    }
}