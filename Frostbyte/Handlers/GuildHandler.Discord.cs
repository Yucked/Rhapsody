using Frostbyte.Entities.Discord;
using Frostbyte.Extensions;
using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Frostbyte.Handlers
{
    public sealed partial class GuildHandler
    {
        private async Task HandleDiscordOpAsync(BaseDiscordPayload payload)
        {
            switch (payload.OpCode)
            {
                case 2:
                    VoiceReadyPayload = payload.Data.TryCast<VoiceReadyPayload>();
                    var basePayload = new BaseDiscordPayload(1, new SelectPayload(VoiceReadyPayload.IPAddress, VoiceReadyPayload.Port));
                    await _socket.SendAsync(basePayload);
                    break;

                case 4:
                    SessionDescriptionPayload = payload.Data.TryCast<SessionDescriptionPayload>();
                    if (SessionDescriptionPayload.Mode != "xsalsa20_poly1305")
                        return;

                    try
                    {
                        UdpClient = new UdpClient();
                        UdpClient.Connect(VoiceReadyPayload.IPAddress, VoiceReadyPayload.Port);
                        await HandleUdpAsync().ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        LogHandler<GuildHandler>.Log.Error(ex?.InnerException ?? ex);
                    }

                    IsReady = true;
                    break;

                case 8:
                    var helloPayload = payload.Data.TryCast<HelloPayload>();
                    if (_heartBeatTask != null)
                    {
                        _heartBeatToken.Cancel(false);
                        _heartBeatTask.Dispose();
                        _heartBeatTask = null;
                    }

                    _heartBeatToken = new CancellationTokenSource();
                    _heartBeatTask = Task.Run(() => HandleHeartbeatAsync(helloPayload.HeartBeatInterval), _heartBeatToken.Token);
                    break;

                default:
                    LogHandler<GuildHandler>.Log.Debug($"Received {payload.OpCode} op code.");
                    break;
            }
        }

        private async Task HandleHeartbeatAsync(int interval)
        {
            while (!_heartBeatToken.IsCancellationRequested)
            {
                await Task.Delay(interval).ConfigureAwait(false);
                var payload = new BaseDiscordPayload(3, DateTimeOffset.UtcNow.ToUnixTimeSeconds());
                await _socket.SendAsync(payload).ConfigureAwait(false);
            }
        }
    }
}