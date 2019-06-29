using Frostbyte.Audio.Codecs.Enums;
using Frostbyte.Entities;
using System;
using System.Runtime.InteropServices;

namespace Frostbyte.Audio.Codecs
{
    public sealed class OpusCodec
    {
        public const int SampleRate = 48000;
        public const int ChannelCount = 2;
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

        public static unsafe void OpusEncode(IntPtr encoder, ReadOnlySpan<byte> pcm, int frameSize, ref Span<byte> opus)
        {
            var len = 0;

            fixed (byte* pcmPtr = &pcm.GetPinnableReference())
            fixed (byte* opusPtr = &opus.GetPinnableReference())
                len = OpusEncode(encoder, pcmPtr, frameSize, opusPtr, opus.Length);

            if (len < 0)
            {
                var error = (OpusError)len;
                throw new Exception($"Could not encode PCM data to Opus: {error} ({(int)error}).");
            }

            opus = opus.Slice(0, len);
        }

        public void Encode(ReadOnlySpan<byte> pcm, ref Span<byte> target)
        {
            var encoder = OpusCreateEncoder(SampleRate, ChannelCount, (int)_settings, out var error);

            if (error != OpusError.Ok)
                throw new Exception($"Could not instantiate Opus encoder: {error} ({(int)error}).");

            if (pcm.Length != target.Length)
                throw new ArgumentException("PCM and Opus buffer lengths need to be equal.", nameof(target));

            var duration = pcm.Length / (SampleRate / 1000) / ChannelCount / 2;
            var frameSize = duration * (SampleRate / 1000);
            var sampleSize = duration * ChannelCount * 2;

            if (pcm.Length != sampleSize)
                throw new ArgumentException("Invalid PCM sample size.", nameof(target));

            OpusEncode(encoder, pcm, frameSize, ref target);
        }
    }
}