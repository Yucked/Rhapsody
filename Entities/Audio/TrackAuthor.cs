using System;

namespace Frostbyte.Entities.Audio
{
    [Serializable]
    public sealed class TrackAuthor
    {
        public string Name { get; set; }

        public string Url { get; set; }

        public string AvatarUrl { get; set; }
    }
}