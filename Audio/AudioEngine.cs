using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Linq;
using System.Net.WebSockets;
using System.Threading.Tasks;
using Frostbyte.Audio.Codecs;
using Frostbyte.Audio.EventArgs;
using Frostbyte.Entities.Audio;
using Frostbyte.Entities.Enums;
using Frostbyte.Entities.Packets;
using Frostbyte.Extensions;
using Frostbyte.Handlers;
using Frostbyte.Websocket;

namespace Frostbyte.Audio
{
    public sealed class AudioEngine : IAsyncDisposable
    {
        public bool IsReady { get; private set; }
        private bool IsPaused { get; set; }

        public bool IsPlaying
            => PlaybackCompleted != null && !PlaybackCompleted.Task.IsCompleted;

        public ConcurrentQueue<AudioPacket> Packets { get; }
        public TaskCompletionSource<bool> PlaybackCompleted { get; set; }

        private ushort _sequence;
        private uint _timeStamp;
        private AudioTrack _currentTrack;

        private readonly AudioStream _stream;
        private readonly CacheHandler _cache;
        private readonly OpusCodec _opusCodec;
        private readonly RtpCodec _rtpCodec;
        private readonly WebSocket _socket;
        private readonly SourceHandler _sources;
        private readonly WsVoiceClient _voiceClient;

        public AudioEngine(WsVoiceClient voiceClient, WebSocket socket)
        {
            _voiceClient = voiceClient;
            _socket = socket;

            _rtpCodec = new RtpCodec();
            _opusCodec = new OpusCodec();

            Packets = new ConcurrentQueue<AudioPacket>();
            _stream = new AudioStream(this);

            _sources = Singleton.Of<SourceHandler>();
            _cache = Singleton.Of<CacheHandler>();

            IsReady = true;
        }

        public void BuildAudioPacket(ReadOnlySpan<byte> pcm, ref Memory<byte> target)
        {
            var rented =
                ArrayPool<byte>.Shared.Rent(
                    AudioHelper.GetRtpPacketSize(AudioHelper.MAX_FRAME_SIZE * AudioHelper.CHANNELS * 2));
            var packet = rented.AsSpan();
            _rtpCodec.EncodeHeader(_sequence, _timeStamp, _voiceClient.Vrp.Ssrc, packet);

            var opus = packet.Slice(RtpCodec.HEADER_SIZE, pcm.Length);
            _opusCodec.Encode(pcm, ref opus);

            _sequence++;
            _timeStamp += (uint) AudioHelper.GetFrameSize(AudioHelper.GetSampleDuration(pcm.Length));

            Span<byte> nonce = stackalloc byte[SodiumCodec.NonceSize];
            _voiceClient.SodiumCodec.GenerateNonce(packet.Slice(0, RtpCodec.HEADER_SIZE), nonce);

            Span<byte> encrypted = stackalloc byte[opus.Length + SodiumCodec.MacSize];
            _voiceClient.SodiumCodec.Encrypt(opus, encrypted, nonce);
            encrypted.CopyTo(packet.Slice(RtpCodec.HEADER_SIZE));
            packet = packet.Slice(0, AudioHelper.GetRtpPacketSize(encrypted.Length));

            target = target.Slice(0, packet.Length);
            packet.CopyTo(target.Span);
            ArrayPool<byte>.Shared.Return(rented);
        }

        public async Task PlayAsync(PlayPacket playPacket)
        {
            _cache.TryGetFromCache(playPacket.Id, out var track);
            if (track is null)
            {
                var (isEnabled, response) = await _sources
                    .HandleRequestAsync(track.Provider, track.Url ?? track.Title)
                    .ConfigureAwait(false);

                if (isEnabled)
                {
                    LogHandler<AudioEngine>.Log
                        .Error($"{track.Provider} is disabled in configuration.");
                    return;
                }

                track = response.Tracks
                    .FirstOrDefault(x => x.Url == track.Url || x.Title.Contains(track.Title));
            }

            if (track is null)
            {
                LogHandler<AudioEngine>.Log.Error($"Unable to play the requested track: {track.Title}");
                return;
            }

            if (playPacket.StartTime > track.Duration || playPacket.StartTime < 0 ||
                playPacket.EndTime > track.Duration)
            {
                LogHandler<AudioEngine>.Log.Error($"Client sent out-of-bounds start or end time.");
                return;
            }

            _currentTrack = track;
            var stream = await _sources.GetStreamAsync(track).ConfigureAwait(false);
            await stream.CopyToAsync(_stream)
                .ConfigureAwait(false);

            await _stream.FlushAsync()
                .ConfigureAwait(false);

            await PlaybackCompleted.Task
                .ConfigureAwait(false);

            await _socket.SendAsync(new OnTrackEndEventArgs
                {
                    Track = track,
                    Reason = TrackEndReason.FINISHED
                })
                .ConfigureAwait(false);

            _currentTrack = default;
        }

        public void Pause(PausePacket pause)
        {
            IsPaused = pause.IsPaused;
        }

        public async Task StopAsync(StopPacket stop)
        {
            PlaybackCompleted.SetResult(true);
            _stream.Close();

            await _socket.SendAsync(new OnTrackEndEventArgs
                {
                    Track = _currentTrack,
                    Reason = TrackEndReason.STOPPED
                })
                .ConfigureAwait(false);
        }

        public void Seek(SeekPacket seek)
        {
        }

        public void EqualizeStream(EqualizerPacket equalizer)
        {
        }

        public void SetVolume(VolumePacket volumePacket)
        {
            if (volumePacket.Value < -1 || volumePacket.Value > 125)
            {
                LogHandler<AudioEngine>.Log.Error($"{volumePacket.GuildId} specified out of bounds value for volume.");
                return;
            }

            _stream.Volume = volumePacket.Value;
        }

        public async ValueTask DisposeAsync()
        {
            IsReady = false;
        }
    }
}