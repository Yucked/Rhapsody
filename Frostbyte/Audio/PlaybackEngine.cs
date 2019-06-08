using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace Frostbyte.Audio
{
    public sealed class PlaybackEngine
    {
        /*
 * https://github.com/naudio/NAudio/blob/master/Docs/WaveProviders.md
 * https://github.com/naudio/NAudio/blob/master/Docs/RawSourceWaveStream.md
 * https://markheath.net/post/fire-and-forget-audio-playback-with
 * https://github.com/naudio/NAudio/blob/master/Docs/SmbPitchShiftingSampleProvider.md
 * https://github.com/naudio/NAudio/blob/master/Docs/FadeInOutSampleProvider.md
 */

        private readonly IWavePlayer outputDevice;
        private readonly BufferedWaveProvider bufferedWave;
        private readonly MixingSampleProvider mixer;
    }
}