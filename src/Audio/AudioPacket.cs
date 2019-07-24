using System;

namespace Frostbyte.Audio
{
    public readonly struct AudioPacket
    {
        public ReadOnlyMemory<byte> Bytes { get; }
        public int MillisecondDuration { get; }
        public bool IsSilence { get; }

        public AudioPacket(ReadOnlyMemory<byte> bytes, int msDuration, bool isSilence = false)
        {
            Bytes = bytes;
            MillisecondDuration = msDuration;
            IsSilence = isSilence;
        }
    }
}