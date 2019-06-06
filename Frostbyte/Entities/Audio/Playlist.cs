using System;

namespace Frostbyte.Entities.Audio
{
    public class Playlist : IAudioItem
    {
        public string Id { get; set; }

        public string Title { get; set; }
        
        public Author Author { get; set; }
        
        public string ThumbnailUrl { get; set; }
        
        public string Url { get; set; }

        public DateTime LastUpdated { get; set; }

        public long Count { get; set; }
    }
}