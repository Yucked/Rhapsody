using System;
using System.Runtime.InteropServices;
using Frostbyte.Entities.Enums;
using Frostbyte.Factories;

namespace Frostbyte.AudioEngine.Codecs
{
    public sealed class OpusCodec
    {
        [DllImport("opus", CallingConvention = CallingConvention.Cdecl, EntryPoint = "opus_encode")]
        private static extern unsafe int OpusEncode(IntPtr encoder, byte* pcmData, int frameSize, byte* data, int maxDataBytes);

        [DllImport("opus", CallingConvention = CallingConvention.Cdecl, EntryPoint = "opus_encoder_create")]
        private static extern IntPtr OpusCreateEncoder(int sampleRate, int channels, int application, out OpusErrorType error);

        private static unsafe void OpusEncode(IntPtr encoder, ReadOnlySpan<byte> pcm, int frameSize,
            ref Span<byte> opus)
        {
            int len;

            fixed (byte* pcmPtr = &pcm.GetPinnableReference())
            fixed (byte* opusPtr = &opus.GetPinnableReference())
            {
                len = OpusEncode(encoder, pcmPtr, frameSize, opusPtr, opus.Length);
            }

            if (len < 0)
            {
                var error = (OpusErrorType) len;
                LogFactory.Error<OpusCodec>($"Could not encode PCM data to Opus -> {error}");
                return;
            }

            opus = opus.Slice(0, len);
        }

        public static bool TryEncode(ReadOnlySpan<byte> pcm, ref Span<byte> target, OpusVoiceType opusVoiceType)
        {
            var encoder = OpusCreateEncoder(AudioHelper.SAMPLE_RATE, AudioHelper.CHANNELS, (int) opusVoiceType, out var error);
            if (error != OpusErrorType.Ok)
            {
                LogFactory.Error<OpusCodec>($"Failed to initialize opus encoder -> {error}");
                return false;
            }

            if (pcm.Length != target.Length)
            {
                LogFactory.Error<OpusCodec>("PCM and Opus lengths aren't the same.");
                return false;
            }

            var duration = AudioHelper.GetSampleDuration(pcm.Length);
            var frameSize = AudioHelper.GetFrameSize(duration);
            var sampleSize = AudioHelper.GetSampleSize(duration);

            if (pcm.Length != sampleSize)
            {
                LogFactory.Error<OpusCodec>("PCM sample isn't valid.");
                return false;
            }

            OpusEncode(encoder, pcm, frameSize, ref target);
            return true;
        }
    }
}