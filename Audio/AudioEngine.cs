using Frostbyte.Entities;
using Frostbyte.Entities.Enums;
using Frostbyte.Entities.Packets;
using Frostbyte.Entities.Results;
using Frostbyte.Extensions;
using Frostbyte.Handlers;
using System;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace Frostbyte.Audio
{
    public sealed class AudioEngine : IAsyncDisposable
    {
        private readonly WebSocket _socket;

        private Task TrackUpdateTask;
        private CancellationTokenSource TrackCancel;
        private CancellationToken TrackToken
            => TrackCancel.Token;

        public bool IsReady { get; set; }
        public bool ToggleCrossfade { get; set; }


        public bool IsPaused { get; private set; }
        public bool IsPlaying { get; private set; }

        public AudioEngine(WebSocket socket)
        {
            _socket = socket;
        }

        public ValueTask DisposeAsync()
        {
            throw new NotImplementedException();
        }
    }
}