using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Frostbyte.Audio
{
    public sealed class AudioStream
    {
        private const int RESAMPLE_RATE = 48000;
        private const int RESAMPLE_CHANNELS = 2;

        public static IWaveProvider GetPCMStream(Stream stream)
        {
            var streamMedia = new StreamMediaFoundationReader(stream) as WaveStream;
            var waveFormat = new WaveFormat(RESAMPLE_RATE, 16, RESAMPLE_CHANNELS);

            if (streamMedia.WaveFormat.SampleRate != RESAMPLE_RATE ||
                streamMedia.WaveFormat.Channels != RESAMPLE_CHANNELS ||
                streamMedia.WaveFormat.BitsPerSample != 16)
                streamMedia = new WaveFormatConversionStream(waveFormat, streamMedia);

            var fade = new FadeInOutSampleProvider(streamMedia.ToSampleProvider(), true);
            fade.BeginFadeIn(2000);
            fade.BeginFadeOut(2000);

            streamMedia.Dispose();

            return new SampleToWaveProvider16(fade);
        }

        public static async Task CreatePCMStreamAsync()
        {

        }
    }
}