using Frostbyte.Audio.Codecs;
using System;
using System.Runtime.InteropServices;

namespace Frostbyte.Audio
{
    public sealed class AudioHelper
    {
        public const int SampleRate
            = 48000;

        public const int Channels
            = 2;

        public const int MaxFrameSize
            = 120 * (SampleRate / 1000);

        public static int GetSampleSize(int duration)
            => duration * Channels * (SampleRate / 1000) * 2;

        public static int GetSampleDuration(int size)
            => size / (SampleRate / 1000) / (Channels / 2);

        public static int GetFrameSize(int duration)
            => duration * (SampleRate / 1000);

        public static int GetRTPPacketSize(int value)
            => RTPCodec.HeaderSize + value;

        public static void ZeroFill(Span<byte> buffer)
        {
            var zero = 0;
            var i = 0;

            for (; i < buffer.Length / 4; i++)
            {
                MemoryMarshal.Write(buffer, ref zero);
            }

            var remainder = buffer.Length % 4;
            if (remainder == 0)
            {
                return;
            }

            for (; i < buffer.Length; i++)
            {
                buffer[i] = 0;
            }
        }
    }
}