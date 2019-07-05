using System;
using System.Runtime.InteropServices;
using Frostbyte.Audio.Codecs;

namespace Frostbyte.Audio
{
    public sealed class AudioHelper
    {
        public const int SAMPLE_RATE
            = 48000;

        public const int CHANNELS
            = 2;

        public const int MAX_FRAME_SIZE
            = 120 * (SAMPLE_RATE / 1000);

        public static int GetSampleSize(int duration)
        {
            return duration * CHANNELS * (SAMPLE_RATE / 1000) * 2;
        }

        public static int GetSampleDuration(int size)
        {
            return size / (SAMPLE_RATE / 1000) / (CHANNELS / 2);
        }

        public static int GetFrameSize(int duration)
        {
            return duration * (SAMPLE_RATE / 1000);
        }

        public static int GetRtpPacketSize(int value)
        {
            return RtpCodec.HEADER_SIZE + value;
        }

        public static void ZeroFill(Span<byte> buffer)
        {
            var zero = 0;
            var i = 0;

            for (; i < buffer.Length / 4; i++) MemoryMarshal.Write(buffer, ref zero);

            var remainder = buffer.Length % 4;
            if (remainder == 0) return;

            for (; i < buffer.Length; i++) buffer[i] = 0;
        }
    }
}