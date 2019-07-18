using System;

namespace Frostbyte.Audio
{
    public struct AudioPacket
    {
        public bool IsSilence { get; set; }
        public int MillisecondDuration { get; set; }
        public ReadOnlyMemory<byte> Bytes { get; set; }
    }
}