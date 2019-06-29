﻿using Frostbyte.Audio;
using Frostbyte.Entities.Discord;
using Frostbyte.Entities.Enums;
using Frostbyte.Entities.Packets;
using Frostbyte.Extensions;
using Frostbyte.Handlers;
using System;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Frostbyte.Websocket
{
    public sealed class WSVoiceClient : IAsyncDisposable
    {
        private readonly ulong _guildId;
        private readonly WebSocket _clientSocket;
        private readonly CancellationTokenSource _mainCancel, _receiveCancel, _heartBeatCancel;

        private ClientWebSocket _socket;
        private SessionDescriptionPayload _sdp;
        private Task _receiveTask, _heartBeatTask, _keepAliveTask;
        private UdpClient _udp;
        private VoiceReadyPayload _vrp;

        public PlaybackEngine Engine { get; }

        public WSVoiceClient(ulong guildId, WebSocket clientSocket)
        {
            _guildId = guildId;
            _clientSocket = clientSocket;
            _socket = new ClientWebSocket();
            _receiveCancel = new CancellationTokenSource();
            _heartBeatCancel = new CancellationTokenSource();
            _mainCancel = CancellationTokenSource.CreateLinkedTokenSource(_receiveCancel.Token, _heartBeatCancel.Token);

            Engine = new PlaybackEngine(clientSocket);
        }

        public async Task HandleVoiceUpdateAsync(VoiceUpdatePacket voiceUpdate)
        {
            if (_socket != null && _socket.State != WebSocketState.None && _socket.State != WebSocketState.Closed)
            {
                await _socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Restarting.", CancellationToken.None)
                             .ConfigureAwait(false);
            }

            try
            {
                await _socket.ConnectAsync(new Uri($"wss://{voiceUpdate.EndPoint}/?v=3"), CancellationToken.None)
                             .ContinueWith(x => VerifyConnectionAsync(voiceUpdate, x))
                             .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                LogHandler<WSVoiceClient>.Log.Error(ex?.InnerException ?? ex);
            }
        }

        private async Task VerifyConnectionAsync(VoiceUpdatePacket packet, Task task)
        {
            if (task.IsCanceled || task.IsFaulted || task.Exception != null)
            {
                LogHandler<WSVoiceClient>.Log.Error(task.Exception);
            }
            else
            {
                _receiveTask = Task.Run(ReceiveTaskAsync, _receiveCancel.Token);
                var payload = new BaseDiscordPayload(VoiceOPType.Identify, new IdentifyPayload(packet.GuildId, packet.UserId, packet.SessionId, packet.Token));
                await _socket.SendAsync(payload).ConfigureAwait(false);
            }
        }

        private async Task ReceiveTaskAsync()
        {
            try
            {
                while (!_receiveCancel.IsCancellationRequested && _socket.State == WebSocketState.Open)
                {
                    var memory = new Memory<byte>();
                    var result = await _socket.ReceiveAsync(memory, CancellationToken.None).ConfigureAwait(false);
                    switch (result.MessageType)
                    {
                        case WebSocketMessageType.Close:
                            await _socket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None)
                                         .ConfigureAwait(false);
                            break;

                        case WebSocketMessageType.Text:
                            var parse = JsonSerializer.Parse<BaseDiscordPayload>(memory.Span);
                            await ProcessPayloadAsync(parse).ConfigureAwait(false);
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                LogHandler<WSVoiceClient>.Log.Error(ex?.InnerException ?? ex);
            }
            finally
            {
                _socket?.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None).ConfigureAwait(false);
                _socket?.Dispose();
            }
        }

        private async Task ProcessPayloadAsync(BaseDiscordPayload payload)
        {
            switch (payload.OP)
            {
                case VoiceOPType.Ready: //Create UDP connection and initialize heartbeat task.
                    LogHandler<WSVoiceClient>.Log.Debug("Received voice ready payload.");

                    _vrp = payload.Data.TryCast<VoiceReadyPayload>();
                    _udp = new UdpClient(_vrp.IPAddress, _vrp.Port);
                    await _udp.SendDiscoveryAsync(_vrp.SSRC).ConfigureAwait(false);
                    LogHandler<WSVoiceClient>.Log.Debug($"Sent UDP discovery with {_vrp.SSRC} ssrc.");

                    _heartBeatTask = HandleHeartbeatAsync(_vrp.HeartbeatInterval);
                    LogHandler<WSVoiceClient>.Log.Debug($"Started heartbeat task with {_vrp.HeartbeatInterval} interval.");

                    Engine.IsReady = true;
                    LogHandler<WSVoiceClient>.Log.Debug($"Guild {_guildId} voice connection is ready.");
                    break;

                case VoiceOPType.SessionDescription:
                    LogHandler<WSVoiceClient>.Log.Debug("Received voice ready payload.");
                    var sdp = payload.Data.TryCast<SessionDescriptionPayload>();
                    if (sdp.Mode != "xsalsa20_poly1305")
                        return;

                    _sdp = sdp;
                    await _socket.SendAsync(new BaseDiscordPayload(VoiceOPType.Speaking, new
                    {
                        delay = 0,
                        speaking = false
                    })).ConfigureAwait(false);

                    _keepAliveTask = SendKeepAliveAsync();
                    break;

                case VoiceOPType.Hello:
                    var helloPayload = payload.Data.TryCast<HelloPayload>();
                    if (_heartBeatTask != null)
                    {
                        _heartBeatCancel.Cancel(false);
                        _heartBeatTask.Dispose();
                        _heartBeatTask = null;
                    }

                    _heartBeatTask = HandleHeartbeatAsync(helloPayload.HeartBeatInterval);
                    break;

                case VoiceOPType.Speaking:
                    break;

                default:
                    LogHandler<WSVoiceClient>.Log.Debug($"Received {payload.OP} op code.");
                    break;
            }
        }

        private async Task HandleHeartbeatAsync(int interval)
        {
            while (!_heartBeatCancel.IsCancellationRequested)
            {
                var payload = new BaseDiscordPayload(VoiceOPType.Heartbeat, DateTimeOffset.UtcNow.ToUnixTimeSeconds());
                await _socket.SendAsync(payload).ConfigureAwait(false);
                await Task.Delay(interval, _heartBeatCancel.Token).ConfigureAwait(false);
            }
        }

        private async Task SendKeepAliveAsync()
        {
            var keepAlive = 0;
            while (!_mainCancel.IsCancellationRequested)
            {
                await _udp.SendKeepAliveAsync(ref keepAlive).ConfigureAwait(false);
                await Task.Delay(4500, _mainCancel.Token).ConfigureAwait(false);
            }
        }

        public async ValueTask DisposeAsync()
        {
            _udp.Close();
            _heartBeatCancel.Cancel(false);
            _receiveCancel.Cancel(true);
            _heartBeatTask?.Dispose();
            _receiveTask?.Dispose();
            await _socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Close requested.", CancellationToken.None)
                .ConfigureAwait(false);
            _socket.Dispose();
        }
    }
}