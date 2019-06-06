using Frostbyte.Entities.Discord;
using Frostbyte.Entities.Packets;
using Frostbyte.Extensions;
using System;
using System.Net.WebSockets;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Frostbyte.Handlers
{
    public sealed partial class GuildHandler
    {
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

                case DestroyPacket destroy:
                    break;

                case SeekPacket seek:
                    break;

                case EqualizerPacket equalizer:
                    break;

                case VoiceUpdatePacket voiceUpdate:
                    if (string.IsNullOrWhiteSpace(voiceUpdate.EndPoint))
                        return;

                    if (_socket != null && _socket.State != WebSocketState.None && _socket.State != WebSocketState.Closed)
                    {
                        await _socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Restarting.", CancellationToken.None)
                                     .ConfigureAwait(false);
                    }

                    _socket = new ClientWebSocket();
                    try
                    {
                        await _socket.ConnectAsync(new Uri($"wss://{voiceUpdate.EndPoint}/?v=3"), CancellationToken.None)
                                     .ContinueWith(x => VerifyConnectionAsync(voiceUpdate, x))
                                     .ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        LogHandler<GuildHandler>.Log.Error(ex?.InnerException ?? ex);
                    }

                    break;
            }
        }

        private async Task VerifyConnectionAsync(VoiceUpdatePacket packet, Task task)
        {
            if (task.IsCanceled || task.IsFaulted || task.Exception != null)
            {
                LogHandler<GuildHandler>.Log.Error(task.Exception);
            }
            else
            {
                _receiveToken ??= new CancellationTokenSource();
                _receiveTask = Task.Run(ReceiveTaskAsync, _receiveToken.Token);
                var payload = new BaseDiscordPayload(0, new IdentifyPayload(_guildId, _userId, packet.SessionId, packet.Token));
                await _socket.SendAsync(payload).ConfigureAwait(false);
            }
        }

        private async Task ReceiveTaskAsync()
        {
            try
            {
                while (!_receiveToken.IsCancellationRequested && _socket.State == WebSocketState.Open)
                {
                    var memory = new Memory<byte>();
                    var result = await _socket.ReceiveAsync(memory, CancellationToken.None).ConfigureAwait(false);
                    switch (result.MessageType)
                    {
                        case WebSocketMessageType.Close:
                            await _socket.CloseAsync(WebSocketCloseStatus.NormalClosure, String.Empty, CancellationToken.None)
                                         .ConfigureAwait(false);
                            break;

                        case WebSocketMessageType.Text:
                            var parse = JsonSerializer.Parse<BaseDiscordPayload>(memory.Span);
                            await HandleDiscordOpAsync(parse);
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                LogHandler<GuildHandler>.Log.Error(ex?.InnerException ?? ex);
            }
            finally
            {
                _socket?.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None).ConfigureAwait(false);
                _socket?.Dispose();
            }
        }
    }
}