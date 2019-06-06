using System;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Frostbyte.Entities.Discord;

namespace Frostbyte.Handlers
{
    public sealed partial class GuildHandler : IAsyncDisposable
    {
        public bool IsPlaying { get; private set; }
        public event Func<ulong, bool> OnClosed;

        private readonly int _shards;
        private readonly ulong _guildId, _userId;


        private ClientWebSocket _socket;
        private CancellationTokenSource _mainToken, _heartBeatToken, _receiveToken;
        private Task _receiveTask, _heartBeatTask;
        private UdpClient UdpClient;
        private VoiceReadyPayload VoiceReadyPayload;

        public GuildHandler(ulong guildId, ulong userId, int shards)
        {
            _shards = shards;
            _guildId = guildId;
            _userId = userId;
            _heartBeatToken = new CancellationTokenSource();
            _receiveToken = new CancellationTokenSource();
            _mainToken = CancellationTokenSource.CreateLinkedTokenSource(_heartBeatToken.Token, _receiveToken.Token);
        }

        public async ValueTask DisposeAsync()
        {
            UdpClient.Close();
            UdpClient.Dispose();
            _mainToken.Cancel(false);
            _heartBeatTask.Dispose();
            _receiveTask.Dispose();
        }
    }
}