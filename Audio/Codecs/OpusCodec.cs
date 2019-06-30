using Frostbyte.Audio.Codecs.Enums;
using Frostbyte.Entities;
using Frostbyte.Handlers;
using System;
using System.Runtime.InteropServices;

namespace Frostbyte.Audio.Codecs
{
    public sealed class OpusCodec
    {
        private readonly VoiceSettings _settings;

        public OpusCodec()
        {
            _settings = Singleton.Of<Configuration>().VoiceSettings;
        }

        [DllImport("opus", CallingConvention = CallingConvention.Cdecl, EntryPoint = "opus_encode")]
        private static unsafe extern int OpusEncode(IntPtr encoder, byte* pcmData, int frameSize, byte* data, int maxDataBytes);

        [DllImport("opus", CallingConvention = CallingConvention.Cdecl, EntryPoint = "opus_encoder_ctl")]
        private static extern OpusError OpusEncoderControl(IntPtr encoder, OpusControl request, int value);

        [DllImport("opus", CallingConvention = CallingConvention.Cdecl, EntryPoint = "opus_encoder_create")]
        private static extern IntPtr OpusCreateEncoder(int sampleRate, int channels, int application, out OpusError error);

        private static unsafe void OpusEncode(IntPtr encoder, ReadOnlySpan<byte> pcm, int frameSize, ref Span<byte> opus)
        {
            var len = 0;

            fixed (byte* pcmPtr = &pcm.GetPinnableReference())
            fixed (byte* opusPtr = &opus.GetPinnableReference())
                len = OpusEncode(encoder, pcmPtr, frameSize, opusPtr, opus.Length);

            if (len < 0)
            {
                var error = (OpusError)len;
                LogHandler<OpusCodec>.Log.Error($"Could not encode PCM data to Opus -> {error}");
                return;
            }

            opus = opus.Slice(0, len);
        }

        public void Encode(ReadOnlySpan<byte> pcm, ref Span<byte> target)
        {
            var encoder = OpusCreateEncoder(AudioHelper.SampleRate, AudioHelper.Channels, (int)_settings, out var error);

            if (error != OpusError.Ok)
            {
                LogHandler<OpusCodec>.Log.Error($"Failed to initialize opus encoder -> {error}");
                return;
            }

            if (pcm.Length != target.Length)
            {
                LogHandler<OpusCodec>.Log.Error("PCM and Opus lengths aren't the same.");
                return;
            }

            var duration = AudioHelper.GetSampleDuration(pcm.Length);
            var frameSize = AudioHelper.GetFrameSize(duration);
            var sampleSize = AudioHelper.GetSampleSize(duration);

            if (pcm.Length != sampleSize)
            {
                LogHandler<OpusCodec>.Log.Error("PCM sample isn't valid.");
                return;
            }

            OpusEncode(encoder, pcm, frameSize, ref target);
        }
    }
}