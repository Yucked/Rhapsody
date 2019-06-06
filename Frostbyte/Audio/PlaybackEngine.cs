using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Frostbyte.Audio
{
    public sealed class PlaybackEngine
    {
        private readonly IWavePlayer outputDevice;
        private readonly BufferedWaveProvider bufferedWave;
        private readonly MixingSampleProvider mixer;


        /*
         * https://github.com/naudio/NAudio/blob/master/Docs/WaveProviders.md
         * https://github.com/naudio/NAudio/blob/master/Docs/RawSourceWaveStream.md
         * https://markheath.net/post/fire-and-forget-audio-playback-with
         * https://github.com/naudio/NAudio/blob/master/Docs/SmbPitchShiftingSampleProvider.md
         * https://github.com/naudio/NAudio/blob/master/Docs/FadeInOutSampleProvider.md
         */
    }
}