using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Frostbyte.Entities.Packets;
using Frostbyte.Extensions;
using Frostbyte.Handlers;

namespace Frostbyte.Websocket
{
    public sealed class WsClient
    {
        public bool IsDisposed { get; private set; }
        public ConcurrentDictionary<ulong, WsVoiceClient> VoiceClients { get; private set; }

        private readonly CancellationTokenSource _receiveCancel;
        private readonly WebSocket _socket;
        private ReadyPacket _readyPacket;

        public WsClient(WebSocketContext socketContext)
        {
            _socket = socketContext.WebSocket;
            VoiceClients = new ConcurrentDictionary<ulong, WsVoiceClient>();
            _receiveCancel = new CancellationTokenSource();

            _socket.ReceiveAsync<WsClient, PlayerPacket>(_receiveCancel, ProcessPacketAsync)
                .ContinueWith(DisposeAsync)
                .ConfigureAwait(false);
        }

        public async Task SendStatsAsync(StatisticPacket stats)
        {
            await _socket.SendAsync(stats)
                .ConfigureAwait(false);
        }

        private async Task ProcessPacketAsync(PlayerPacket packet)
        {
            if (_readyPacket is null && !(packet is ReadyPacket))
            {
                LogHandler<WsClient>.Log.Error($"{packet.GuildId} guild didn't send a ReadyPayload.");
                return;
            }

            _readyPacket = packet as ReadyPacket;
            var vClient = new WsVoiceClient(_socket);
            VoiceClients.TryAdd(packet.GuildId, vClient);

            LogHandler<WsClient>.Log.Debug($"{packet.GuildId} client and engine has been initialized.");

            var voiceClient = VoiceClients[packet.GuildId];

            switch (packet)
            {
                case PlayPacket play:
                    await voiceClient.Engine.PlayAsync(play)
                        .ConfigureAwait(false);
                    break;

                case PausePacket pause:
                    voiceClient.Engine.Pause(pause);
                    break;

                case StopPacket stop:
                    await voiceClient.Engine.StopAsync(stop)
                        .ConfigureAwait(false);
                    break;

                case DestroyPacket destroy:
                    await voiceClient.Engine.DisposeAsync()
                        .ConfigureAwait(false);
                    break;

                case SeekPacket seek:
                    voiceClient.Engine.Seek(seek);
                    break;

                case EqualizerPacket equalizer:
                    voiceClient.Engine.EqualizeStream(equalizer);
                    break;

                case VolumePacket volume:
                    voiceClient.Engine.SetVolume(volume);
                    break;

                case VoiceUpdatePacket voiceUpdate:
                    await VoiceClients[packet.GuildId].HandleVoiceUpdateAsync(voiceUpdate)
                        .ConfigureAwait(false);
                    break;
            }
        }

        private async ValueTask DisposeAsync()
        {
            _receiveCancel.Cancel(false);
            _receiveCancel.Dispose();

            foreach (var (key, value) in VoiceClients)
            {
                await value.DisposeAsync()
                    .ConfigureAwait(false);
                VoiceClients.TryRemove(key, out _);
            }

            VoiceClients.Clear();
            VoiceClients = null;

            await _socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Disposing client.", CancellationToken.None)
                .ConfigureAwait(false);
            _socket.Dispose();

            IsDisposed = true;
        }
    }
}