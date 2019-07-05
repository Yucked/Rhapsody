using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Linq;
using System.Net.WebSockets;
using System.Threading.Tasks;
using Frostbyte.Audio.Codecs;
using Frostbyte.Entities.Packets;
using Frostbyte.Extensions;
using Frostbyte.Handlers;
using Frostbyte.Websocket;

namespace Frostbyte.Audio
{
    public sealed class AudioEngine : IAsyncDisposable
    {
        private readonly CacheHandler _cache;
        private readonly OpusCodec _opusCodec;
        private readonly RtpCodec _rtpCodec;
        private readonly WebSocket _socket;

        private readonly SourceHandler _sources;
        private readonly WsVoiceClient _voiceClient;

        private ushort _sequence;
        private uint _timeStamp;

        public AudioEngine(WsVoiceClient voiceClient, WebSocket socket)
        {
            _voiceClient = voiceClient;
            _socket = socket;

            _rtpCodec = new RtpCodec();
            _opusCodec = new OpusCodec();

            Packets = new ConcurrentQueue<AudioPacket>();
            AudioStream = new AudioStream(this);

            _sources = Singleton.Of<SourceHandler>();
            _cache = Singleton.Of<CacheHandler>();
        }

        public bool IsReady { get; }
        public AudioStream AudioStream { get; }
        public bool IsPaused { get; private set; }

        public bool IsPlaying
            => PlaybackCompleted != null && !PlaybackCompleted.Task.IsCompleted;

        public ConcurrentQueue<AudioPacket> Packets { get; }
        public TaskCompletionSource<bool> PlaybackCompleted { get; set; }

        public ValueTask DisposeAsync()
        {
            throw new NotImplementedException();
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
            string provider;

            if (_cache.TryGetFromCache(playPacket.Hash, out var track))
            {
                if (playPacket.StartTime > track.Duration)
                {
                    LogHandler<AudioEngine>.Log.Error($"{playPacket.GuildId} specified out of range start time.");
                    return;
                }

                provider = track.Hash.DecodeHash().Provider;
            }
            else
            {
                var decode = playPacket.Hash.DecodeHash();
                provider = decode.Provider;
                var request = await _sources.HandleRequestAsync(provider, decode.Url ?? decode.Title)
                    .ConfigureAwait(false);

                if (!request.IsEnabled)
                    return;

                track = request.Response.Tracks.FirstOrDefault();
            }

            var stream = await _sources.GetStreamAsync(provider, track).ConfigureAwait(false);
            await stream.CopyToAsync(AudioStream)
                .ConfigureAwait(false);

            await AudioStream.FlushAsync()
                .ConfigureAwait(false);
        }
    }
}