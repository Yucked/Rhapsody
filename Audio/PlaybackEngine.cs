using Frostbyte.Entities;
using Frostbyte.Entities.Enums;
using Frostbyte.Entities.Packets;
using Frostbyte.Entities.Results;
using Frostbyte.Extensions;
using Frostbyte.Handlers;
using NAudio.Wave;
using System;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace Frostbyte.Audio
{
    public sealed class PlaybackEngine : IAsyncDisposable
    {
        private readonly WaveOutEvent waveOut;
        private readonly WebSocket _socket;
        private StreamMediaFoundationReader streamMedia;
        private Task TrackUpdateTask;
        private CancellationTokenSource TrackCancel;

        public bool IsReady { get; set; }
        public bool IsPaused { get; private set; }
        public bool IsPlaying { get; private set; }
        public bool ToggleCrossfade { get; set; }

        public PlaybackEngine(WebSocket socket)
        {
            waveOut = new WaveOutEvent();
            waveOut.PlaybackStopped += OnPlaybackStopped;
        }

        public async Task PlayAsync(PlayPacket play)
        {
            var source = Singleton.Of<SourceHandler>();
            string provider;

            if (Singleton.Of<CacheHandler>().TryGetFromCache(play.Hash, out var track))
            {
                if (play.StartTime > track.Duration)
                {
                    LogHandler<PlaybackEngine>.Log.RawLog(LogLevel.Error, $"{play.GuildId} specified out of range start time.", default);
                    return;
                }

                provider = track.Hash.DecodeHash().Provider;
            }
            else
            {
                var decode = play.Hash.DecodeHash();
                provider = decode.Provider;
                var request = await source.HandleRequestAsync(provider, decode.Url ?? decode.Title).ConfigureAwait(false);

                track = (request.AdditionObject as SearchResult).Tracks.FirstOrDefault();
            }


            var stream = await source.GetStreamAsync(provider, track).ConfigureAwait(false);

            if (stream?.Length is 0)
            {
                LogHandler<PlaybackEngine>.Log.RawLog(LogLevel.Error, $"{nameof(source)} returned a default stream.", default);
                return;
            }

            streamMedia = new StreamMediaFoundationReader(stream);
            if (play.StartTime.HasValue)
                streamMedia.Skip(play.StartTime.Value);

            waveOut.Init(streamMedia);
            waveOut.Play();
            IsPlaying = true;
            TrackCancel = new CancellationTokenSource((int)(TimeSpan.FromSeconds(5).TotalMilliseconds +
                (play.EndTime.HasValue ? play.EndTime.Value : track.Duration)));
            TrackUpdateTask = Task.Run(() => SendTrackUpdateAsync(track.Hash), TrackCancel.Token);
        }

        public void Pause(PausePacket pause)
        {
            if (pause.IsPaused)
            {
                IsPaused = true;
                waveOut.Pause();
            }
            else
            {
                IsPaused = false;
                waveOut.Play();
            }
        }

        public void Stop(StopPacket stop)
        {
            if (IsPlaying)
                return;

            IsPlaying = false;
            waveOut.Stop();
            TrackCancel.Cancel(false);
        }

        public void Volume(VolumePacket volume)
        {
            if (volume.Value < 0 || volume.Value > 150)
                return;

            waveOut.Volume = volume.Value;
        }

        public void Seek(SeekPacket seek)
        {
            if (seek.Position > streamMedia.Length ||
                seek.Position < streamMedia.Length)
                return;

            streamMedia.Skip((int)seek.Position);
        }

        public async Task EqualizeAsync()
        {

        }

        public ValueTask DisposeAsync()
        {
            IsPlaying = false;
            IsReady = false;
            return default;
        }

        private async void OnPlaybackStopped(object sender, StoppedEventArgs e)
        {
            var response = new ResponseEntity();

            if (e.Exception != null)
            {
                response.IsSuccess = false;
                response.Reason = e.Exception?.InnerException.Message ?? e.Exception.Message;
                response.Operation = OperationType.TrackErrored;
            }
            else
            {
                response.IsSuccess = true;
                response.Reason = "Finished playback.";
                response.Operation = OperationType.TrackFinished;
            }

            IsPlaying = false;
            TrackCancel.Cancel(false);
            TrackCancel.Dispose();
            TrackUpdateTask = null;

            await _socket.SendAsync(response).ConfigureAwait(false);
        }

        private async Task SendTrackUpdateAsync(string hash)
        {
            var response = new ResponseEntity
            {
                Operation = OperationType.TrackUpdate,
                IsSuccess = true
            };

            while (waveOut.PlaybackState is PlaybackState.Playing && !TrackCancel.IsCancellationRequested)
            {
                var update = new
                {
                    streamMedia.Position,
                    Hash = hash
                };
                response.AdditionObject = update;
                await _socket.SendAsync(response).ConfigureAwait(false);
                await Task.Delay(TimeSpan.FromSeconds(5)).ConfigureAwait(false);
            }
        }
    }
}