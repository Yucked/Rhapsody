using Frostbyte.Entities.Audio;
using Frostbyte.Entities.Enums;

namespace Frostbyte.Audio.EventArgs
{
    public struct OnTrackEndEventArgs
    {
        public AudioTrack Track { get; set; }
        public TrackEndReason Reason { get; set; }
    }
}