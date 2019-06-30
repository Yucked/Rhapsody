using Frostbyte.Entities;
using Frostbyte.Entities.Enums;
using Frostbyte.Entities.Packets;
using Frostbyte.Entities.Results;
using Frostbyte.Extensions;
using Frostbyte.Handlers;
using Frostbyte.Websocket;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace Frostbyte.Audio
{
    public sealed class AudioEngine : IAsyncDisposable
    {
        private Task TrackUpdateTask;
        private CancellationTokenSource TrackCancel;
        private CancellationToken TrackToken
            => TrackCancel.Token;

        public bool IsReady { get; }
        public AudioStream AudioStream { get; }
        public bool IsPaused { get; private set; }
        public bool IsPlaying
            => PlaybackCompleted != null && !PlaybackCompleted.Task.IsCompleted;

        public ConcurrentQueue<AudioPacket> Packets { get; }
        public TaskCompletionSource<bool> PlaybackCompleted { get; set; }

        private readonly WebSocket _socket;
        private readonly WSVoiceClient _voiceClient;

        public AudioEngine(WSVoiceClient voiceClient, WebSocket socket)
        {
            _voiceClient = voiceClient;
            _socket = socket;
            Packets = new ConcurrentQueue<AudioPacket>();
            AudioStream = new AudioStream(this);
        }

        public async Task PlayAsync(PlayPacket playPacket)
        {

            await _voiceClient.SendSpeakingAsync(true)
                .ConfigureAwait(false);

            var stream = default(Stream);
            await stream.CopyToAsync(AudioStream)
                .ConfigureAwait(false);

            await AudioStream.FlushAsync()
                .ConfigureAwait(false);
        }

        public ValueTask DisposeAsync()
        {
            throw new NotImplementedException();
        }
    }
}