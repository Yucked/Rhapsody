using System;

namespace Frostbyte.Audio
{
    public readonly struct AudioPacket
    {
        public ReadOnlyMemory<byte> Bytes { get; }
        public int MsDuration { get; }
        public bool IsSilence { get; }

        public AudioPacket(ReadOnlyMemory<byte> bytes, int msDuration, bool isSilence = false)
        {
            Bytes = bytes;
            MsDuration = msDuration;
            IsSilence = isSilence;
        }
    }
}