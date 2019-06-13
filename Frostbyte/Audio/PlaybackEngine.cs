using Frostbyte.Entities.Enums;
using Frostbyte.Entities.Packets;
using Frostbyte.Extensions;
using Frostbyte.Handlers;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System.Threading.Tasks;

namespace Frostbyte.Audio
{
    public sealed class PlaybackEngine
    {
        /***
         * https://github.com/naudio/NAudio/blob/master/Docs/WaveProviders.md
         * https://github.com/naudio/NAudio/blob/master/Docs/RawSourceWaveStream.md
         * https://markheath.net/post/fire-and-forget-audio-playback-with
         * https://github.com/naudio/NAudio/blob/master/Docs/SmbPitchShiftingSampleProvider.md
         * https://github.com/naudio/NAudio/blob/master/Docs/FadeInOutSampleProvider.md
         * ***/

        private readonly IWavePlayer outputDevice;
        private readonly BufferedWaveProvider bufferedWave;
        private readonly MixingSampleProvider mixer;

        public bool IsReady { get; set; }
        public bool IsPaused { get; private set; }
        public bool IsPlaying { get; private set; }

        public PlaybackEngine()
        {
            outputDevice.Init(bufferedWave);
            outputDevice.PlaybackStopped += OnPlaybackStopped;
        }

        public async Task PlayAsync(PlayPacket play)
        {
            if (!Singletons.Cache.TryGetFromCache(play.Hash, out var track))
            {
                if (play.StartTime > track.Duration)
                {
                    LogHandler<PlaybackEngine>.Log.RawLog(LogLevel.Error, $"{play.GuildId} specified out of range start time.", default);
                    return;
                }
            }

            var decode = play.Hash.DecodeHash();
            // GET TRACK FROM SOURCE HANDLER AND PLAY IT
            outputDevice.Play();
        }

        private void OnPlaybackStopped(object sender, StoppedEventArgs e)
        {
            IsPlaying = false;
        }

        public Task PauseAsync(PausePacket pause)
        {
            outputDevice.Pause();
            IsPaused = pause.IsPaused;
            return Task.CompletedTask;
        }

        public Task StopAsync(StopPacket stop)
        {
            outputDevice.Stop();
            IsPlaying = false;
            return Task.CompletedTask;
        }

        public Task VolumeAsync(VolumePacket volume)
        {
            //outputDevice.Volume = volume;
            return Task.CompletedTask;
        }

        public async Task DestroyAsync()
        {

        }

        public async Task SeekAsync()
        {
        }

        public async Task EqualizeAsync()
        {

        }
    }
}