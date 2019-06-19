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
        private CancellationTokenSource TrackSource;

        public bool IsReady { get; private set; }
        public bool IsPaused { get; private set; }
        public bool IsPlaying { get; private set; }

        public PlaybackEngine(bool isReady, WebSocket socket)
        {
            IsReady = isReady;
            _socket = socket;
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
            streamMedia = new StreamMediaFoundationReader(stream);
            streamMedia.Skip(play.StartTime);

            waveOut.Init(streamMedia);
            waveOut.Play();

            IsPlaying = true;
            TrackSource = new CancellationTokenSource((int)(TimeSpan.FromSeconds(5).TotalMilliseconds + play.EndTime is 0 ? track.Duration : play.EndTime));
            TrackUpdateTask = Task.Run(() => SendTrackUpdateAsync(track.Hash), TrackSource.Token);
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
            TrackSource.Cancel(false);
            TrackSource.Dispose();
            TrackUpdateTask = null;

            await _socket.SendAsync(response).ConfigureAwait(false);
        }

        public Task PauseAsync(PausePacket pause)
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
            return Task.CompletedTask;
        }

        public Task StopAsync(StopPacket stop)
        {
            IsPlaying = false;
            waveOut.Stop();
            return Task.CompletedTask;
        }

        public Task VolumeAsync(VolumePacket volume)
        {
            waveOut.Volume = volume.Volume;
            return Task.CompletedTask;
        }

        public Task SeekAsync(SeekPacket seek)
        {
            streamMedia.Skip((int)seek.Position);
            return Task.CompletedTask;
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

        private async Task SendTrackUpdateAsync(string hash)
        {
            var response = new ResponseEntity
            {
                Operation = OperationType.TrackUpdate,
                IsSuccess = true
            };

            while (waveOut.PlaybackState is PlaybackState.Playing && !TrackSource.IsCancellationRequested)
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