using Frostbyte.Handlers;
using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Frostbyte.Entities.Packets;
using Frostbyte.Entities.Enums;
using Frostbyte.Extensions;

namespace Frostbyte.Websocket
{
    public sealed class WSClient
    {
        private ReadyPacket readyPacket;
        private readonly Task _receiveTask;
        private readonly WebSocket _socket;

        public bool IsDisposed { get; private set; }
        public event Func<IPEndPoint, Task> OnClosed;
        public ConcurrentDictionary<ulong, WSVoiceClient> VoiceClients { get; private set; }

        public WSClient(WebSocketContext socketContext)
        {
            _socket = socketContext.WebSocket;
            VoiceClients = new ConcurrentDictionary<ulong, WSVoiceClient>();

            _receiveTask = _socket.ReceiveAsync<WSClient, PlayerPacket>(default, ProcessPacketAsync)
                .ContinueWith(async _ => await DisposeAsync());
        }

        public async Task SendStatsAsync(StatisticPacket stats)
        {
            await _socket.SendAsync(stats)
                .ConfigureAwait(false);
        }

        private async Task ProcessPacketAsync(PlayerPacket packet)
        {
            if (readyPacket is null && !(packet is ReadyPacket))
            {
                LogHandler<WSClient>.Log.RawLog(LogLevel.Critical,
                    $"{packet.GuildId} guild didn't send a ReadyPayload.", default);
                return;
            }
            else
            {
                readyPacket = packet as ReadyPacket;
                var vClient = new WSVoiceClient(packet.GuildId, _socket);
                vClient.Engine.ToggleCrossfade = readyPacket.ToggleCrossfade;
                VoiceClients.TryAdd(packet.GuildId, vClient);

                LogHandler<WSClient>.Log.Debug($"{packet.GuildId} client and engine has been initialized.");
            }

            var voiceClient = VoiceClients[packet.GuildId];

            switch (packet)
            {
                case PlayPacket play:
                    //await voiceClient.Engine.PlayAsync(play).ConfigureAwait(false);
                    break;

                case PausePacket pause:
                    //voiceClient.Engine.Pause(pause);
                    break;

                case StopPacket stop:
                    //voiceClient.Engine.Stop(stop);
                    break;

                case DestroyPacket destroy:
                    await voiceClient.Engine.DisposeAsync().ConfigureAwait(false);
                    break;

                case SeekPacket seek:
                    //voiceClient.Engine.Seek(seek);
                    break;

                case EqualizerPacket equalizer:
                    //await voiceClient.Engine.EqualizeAsync().ConfigureAwait(false);
                    break;

                case VoiceUpdatePacket voiceUpdate:
                    await VoiceClients[packet.GuildId].HandleVoiceUpdateAsync(voiceUpdate).ConfigureAwait(false);
                    break;
            }
        }

        private async ValueTask DisposeAsync()
        {
            foreach (var (key, value) in VoiceClients)
            {
                await value.DisposeAsync().ConfigureAwait(false);
                VoiceClients.TryRemove(key, out _);
            }

            VoiceClients.Clear();
            VoiceClients = null;

            await _socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Disposing client.", CancellationToken.None).ConfigureAwait(false);
            _socket.Dispose();

            IsDisposed = true;
        }
    }
}