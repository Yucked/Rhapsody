namespace Frostbyte.Entities.Infos
{
    public readonly struct TrackInfo
    {
        public string Id { get; }
        public string Title { get; }
        public string Url { get; }
        public long Duration { get; }
        public string ArtworkUrl { get; }
        public bool CanStream { get; }
        public string Provider { get; }
        public AuthorInfo Author { get; }

        public TrackInfo(string id, string title, string url, long duration, string artworkUrl, bool canStream, string provider, AuthorInfo author)
        {
            Id = id;
            Title = title;
            Url = url;
            Duration = duration;
            ArtworkUrl = artworkUrl;
            CanStream = canStream;
            Provider = provider;
            Author = author;
        }
    }
}