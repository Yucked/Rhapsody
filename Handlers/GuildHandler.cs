using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Frostbyte.Entities.Packets;

namespace Frostbyte.Handlers
{
    public sealed class GuildHandler
    {
        public bool IsPlaying { get; set; }

        private readonly int _baseVolume;
        private ClientWebSocket _clientWebSocket;

        public GuildHandler(ulong userId, int shards)
        {
            _baseVolume = 100;
        }

        public async Task HandlePacketAsync(PlayerPacket packet)
        {
            switch (packet)
            {
                case PlayPacket play:
                    break;

                case PausePacket pause:
                    break;

                case StopPacket stop:
                    break;

                case VoiceUpdatePacket voiceUpdate:
                    if (string.IsNullOrWhiteSpace(voiceUpdate.EndPoint))
                        return;

                    if (_clientWebSocket != null && _clientWebSocket.State != WebSocketState.None &&
                        _clientWebSocket.State != WebSocketState.Closed)
                    {
                        await _clientWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Restarting.", CancellationToken.None)
                                              .ConfigureAwait(false);
                    }

                    _clientWebSocket = new ClientWebSocket();
                    try
                    {
                        await _clientWebSocket.ConnectAsync(new Uri($"wss://{voiceUpdate.EndPoint}/?v=3"), CancellationToken.None)
                                              .ConfigureAwait(false);
                    }
                    catch
                    {
                    }

                    break;
            }
        }
    }
}