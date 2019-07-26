using System;
using System.Buffers;
using System.Runtime.InteropServices;
using Frostbyte.Audio.Codecs;

namespace Frostbyte.Audio
{
    public struct AudioHelper
    {
        public static FFmpegPipe Pipe
            => Singleton.Of<FFmpegPipe>();

        public const int SAMPLE_RATE
            = 48000;

        public const int STEREO_CHANNEL
            = 2;

        public const int MAX_FRAME_SIZE
            = 120 * (SAMPLE_RATE / 1000);

        public const int MAX_SILENCE_FRAMES
            = 10;

        public static ReadOnlyMemory<byte> SilenceFrames
            = new byte[] {0xF8, 0xFF, 0xFE};

        public static int GetSampleSize(int duration)
            => duration * STEREO_CHANNEL * (SAMPLE_RATE / 1000) * 2;

        public static int GetSampleDuration(int size)
            => size / (SAMPLE_RATE / 1000) / (STEREO_CHANNEL / 2);

        public static int GetFrameSize(int duration)
            => duration * (SAMPLE_RATE / 1000);

        public static int GetRtpPacketSize(int value)
            => RtpCodec.HEADER_SIZE + value;

        public static ushort Sequence;

        public static uint Timestamp;

        public static void ZeroFill(Span<byte> buffer)
        {
            var zero = 0;
            var i = 0;

            for (; i < buffer.Length / 4; i++)
                MemoryMarshal.Write(buffer, ref zero);

            var remainder = buffer.Length % 4;
            if (remainder == 0)
                return;

            for (; i < buffer.Length; i++)
                buffer[i] = 0;
        }

        public static bool TryPrepareAudioPacket(ReadOnlySpan<byte> pcm, ref Memory<byte> target, uint ssrc,
            ReadOnlyMemory<byte> key)
        {
            var packetArray = ArrayPool<byte>.Shared.Rent(GetRtpPacketSize(MAX_FRAME_SIZE * STEREO_CHANNEL * 2));
            var packetSpan = packetArray.AsSpan();
            if (!RtpCodec.TryEncodeHeader(Sequence, Timestamp, ssrc, packetSpan))
                return false;

            var opusPacket = packetSpan.Slice(RtpCodec.HEADER_SIZE, pcm.Length);
            if (!OpusCodec.TryEncode(pcm, ref opusPacket))
                return false;

            Sequence++;
            Timestamp += (uint) GetFrameSize(GetSampleDuration(pcm.Length));

            Span<byte> nonce = stackalloc byte[SodiumCodec.NonceSize];
            if (SodiumCodec.TryGenerateNonce(packetSpan.Slice(0, RtpCodec.HEADER_SIZE), nonce))
                return false;

            Span<byte> encrypted = stackalloc byte[opusPacket.Length - SodiumCodec.MacSize];
            if (!SodiumCodec.TryEncrypt(opusPacket, encrypted, nonce, key))
                return false;

            encrypted.CopyTo(packetSpan.Slice(RtpCodec.HEADER_SIZE));
            packetSpan = packetSpan.Slice(0, GetRtpPacketSize(encrypted.Length));
            target = target.Slice(0, packetSpan.Length);
            packetSpan.CopyTo(target.Span);
            ArrayPool<byte>.Shared.Return(packetArray);

            return true;
        }
    }
}