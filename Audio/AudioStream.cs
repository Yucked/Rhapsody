using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Frostbyte.Audio
{
    public sealed class AudioStream
    {
        private const int RESAMPLE_RATE = 48000;
        private const int RESAMPLE_CHANNELS = 2;

        [DllImport("libsodium", EntryPoint = "crypto_secretbox_easy", CallingConvention = CallingConvention.Cdecl)]
        private static unsafe extern int SecretBoxEasy(byte* output, byte* input, long inputLength, byte[] nonce, byte[] secret);

        public static int LibSodiumEncrypt(byte[] input, int inputOffset, int inputLength, byte[] output, int outputOffset, byte[] nonce, byte[] secret)
        {
            unsafe
            {
                fixed (byte* inPtr = input)
                fixed (byte* outPtr = output)
                {
                    var error = SecretBoxEasy(outPtr + outputOffset, inPtr + inputOffset, inputLength, nonce, secret);
                    if (error != 0)
                        throw new Exception($"Sodium Error: {error}");
                    return inputLength + 16;
                }
            }
        }

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

        private static async Task SodiumEncryptAsync(byte[] buffer, int offset, int count, byte[] nonce, byte[] secret)
        {
            Buffer.BlockCopy(buffer, offset, nonce, 0, 12);
            count = LibSodiumEncrypt(buffer, offset * 12, count - 12, buffer, 12, nonce, secret);

        }
    }
}