using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Frostbyte.Audio;
using Frostbyte.Audio.Codecs;
using Frostbyte.Entities.Discord;
using Frostbyte.Entities.Enums;
using Frostbyte.Entities.Packets;
using Frostbyte.Extensions;
using Frostbyte.Handlers;

namespace Frostbyte.Websocket
{
    public sealed class WsVoiceClient : IAsyncDisposable
    {
        public ulong GuildId { get;}
        public AudioEngine Engine { get; }
        public SodiumCodec SodiumCodec { get; private set; }
        public VoiceReadyPayload Vrp { get; private set; }

        private readonly CancellationTokenSource _mainCancel, _receiveCancel, _heartBeatCancel;
        private readonly ClientWebSocket _socket;

        private string _hostName;
        private Task _receiveTask, _heartBeatTask;
        private SessionDescriptionPayload _sdp;
        private UdpClient _udp;

        public WsVoiceClient(WebSocket clientSocket, ulong guildId)
        {
            GuildId = guildId;
            _socket = new ClientWebSocket();
            _receiveCancel = new CancellationTokenSource();
            _heartBeatCancel = new CancellationTokenSource();
            _mainCancel = CancellationTokenSource.CreateLinkedTokenSource(_receiveCancel.Token, _heartBeatCancel.Token);

            Engine = new AudioEngine(this, clientSocket);
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

        public async Task HandleVoiceUpdateAsync(VoiceUpdatePacket voiceUpdate)
        {
            if (_socket != null && _socket.State == WebSocketState.Open && _hostName != voiceUpdate.EndPoint)
                await _socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Changing voice server.",
                        CancellationToken.None)
                    .ConfigureAwait(false);

            try
            {
                _hostName = voiceUpdate.EndPoint;
                var url = $"wss://{voiceUpdate.EndPoint}"
                    .WithParameter("encoding", "json")
                    .WithParameter("v", "4")
                    .ToUrl();

                await _socket.ConnectAsync(url, _mainCancel.Token)
                    .ContinueWith(x => VerifyConnectionAsync(voiceUpdate, x))
                    .ConfigureAwait(false);
            }
            catch
            {
                // Ignore
            }
        }

        private async Task VerifyConnectionAsync(VoiceUpdatePacket packet, Task task)
        {
            if (task.VerifyTask())
            {
                LogHandler<WsVoiceClient>.Log.Error(exception: task.Exception);
                return;
            }

            _receiveTask = _socket
                .ReceiveAsync<WsVoiceClient, BaseDiscordPayload>(_receiveCancel, ProcessPayloadAsync)
                .ContinueWith(DisposeAsync);

            var payload = new BaseDiscordPayload(VoiceOpType.Identify,
                new IdentifyPayload
                {
                    ServerId = $"{packet.GuildId}",
                    SessionId = packet.SessionId,
                    UserId = $"{packet.UserId}",
                    Token = packet.Token
                });

            await _socket.SendAsync(payload)
                .ConfigureAwait(false);
        }

        private async Task SendSpeakingAsync(bool isSpeaking)
        {
            var payload = new BaseDiscordPayload(VoiceOpType.Speaking, new
            {
                Speaking = isSpeaking,
                Delay = 0
            });

            await _socket.SendAsync(payload)
                .ConfigureAwait(false);
        }

        private async Task ProcessPayloadAsync(BaseDiscordPayload payload)
        {
            LogHandler<WsVoiceClient>.Log.Debug($"Received {Enum.GetName(typeof(VoiceOpType), payload.Op)} payload.");

            switch (payload.Op)
            {
                case VoiceOpType.Ready:
                    Vrp = payload.Data.TryCast<VoiceReadyPayload>();
                    _udp = new UdpClient(Vrp.IpAddress, Vrp.Port);
                    await _udp.SendDiscoveryAsync(Vrp.Ssrc).ConfigureAwait(false);
                    LogHandler<WsVoiceClient>.Log.Debug($"Sent UDP discovery with {Vrp.Ssrc} ssrc.");

                    _heartBeatTask = HandleHeartbeatAsync(Vrp.HeartbeatInterval);
                    LogHandler<WsVoiceClient>.Log.Debug(
                        $"Started heartbeat task with {Vrp.HeartbeatInterval} interval.");

                    var selectProtocol = new BaseDiscordPayload(VoiceOpType.SelectProtocol,
                        new SelectPayload(Vrp.IpAddress, Vrp.Port));
                    await _socket.SendAsync(selectProtocol)
                        .ConfigureAwait(false);
                    LogHandler<WsVoiceClient>.Log.Debug($"Sent select protocol with {Vrp.IpAddress}:{Vrp.Port}.");

                    _ = VoiceSenderTask();
                    break;

                case VoiceOpType.SessionDescription:
                    var sdp = payload.Data.TryCast<SessionDescriptionPayload>();
                    if (sdp.Mode != "xsalsa20_poly1305")
                        return;

                    _sdp = sdp;
                    SodiumCodec = new SodiumCodec(_sdp.SecretKey);


                    await _socket.SendAsync(new BaseDiscordPayload(VoiceOpType.Speaking, new
                    {
                        delay = 0,
                        speaking = false
                    })).ConfigureAwait(false);


                    _ = SendKeepAliveAsync().ConfigureAwait(false);
                    break;

                case VoiceOpType.Hello:
                    var helloPayload = payload.Data.TryCast<HelloPayload>();
                    if (_heartBeatTask != null)
                    {
                        _heartBeatCancel.Cancel(false);
                        _heartBeatTask.Dispose();
                        _heartBeatTask = null;
                    }

                    _heartBeatTask = HandleHeartbeatAsync(helloPayload.HeartBeatInterval);
                    break;
            }
        }

        private async Task HandleHeartbeatAsync(int interval)
        {
            while (!_heartBeatCancel.IsCancellationRequested)
            {
                var payload = new BaseDiscordPayload(VoiceOpType.Heartbeat, DateTimeOffset.UtcNow.ToUnixTimeSeconds());
                await _socket.SendAsync(payload)
                    .ConfigureAwait(false);
                await Task.Delay(interval, _heartBeatCancel.Token)
                    .ConfigureAwait(false);
            }
        }

        private async Task SendKeepAliveAsync()
        {
            var keepAlive = 0;
            while (!_mainCancel.IsCancellationRequested)
            {
                await _udp.SendKeepAliveAsync(ref keepAlive)
                    .ConfigureAwait(false);
                await Task.Delay(4500, _mainCancel.Token)
                    .ConfigureAwait(false);
            }
        }

        private async Task VoiceSenderTask()
        {
            var synchronizerTicks = (double) Stopwatch.GetTimestamp();
            var synchronizerResolution = Stopwatch.Frequency * 0.005;
            var tickResolution = 10_000_000.0 / Stopwatch.Frequency;

            while (!_mainCancel.IsCancellationRequested)
            {
                var hasPacket = Engine.Packets.TryDequeue(out var packet);
                byte[] packetArray = null;

                if (hasPacket)
                {
                    packetArray = packet.Bytes.ToArray();
                }

                var durationModifier = hasPacket
                    ? packet.MillisecondDuration / 5
                    : 4;
                var cts = Math.Max(Stopwatch.GetTimestamp() - synchronizerTicks, 0);
                if (cts < synchronizerResolution * durationModifier)
                    await Task.Delay(
                            TimeSpan.FromTicks(
                                (long) ((synchronizerResolution * durationModifier - cts) * tickResolution)))
                        .ConfigureAwait(false);

                synchronizerTicks += synchronizerResolution * durationModifier;

                if (!hasPacket || Volatile.Read(ref Engine.IsPlaybackCompleted))
                    continue;

                await SendSpeakingAsync(true).ConfigureAwait(false);
                await _udp.SendAsync(packetArray).ConfigureAwait(false);

                if (!packet.IsSilence && Engine.Packets.Count == 0)
                {
                    var nullpcm = new byte[AudioHelper.GetSampleSize(20)];
                    for (var i = 0; i < 3; i++)
                    {
                        var nullpacket = new byte[nullpcm.Length];
                        var nullpacketmem = nullpacket.AsMemory();

                        Engine.BuildAudioPacket(nullpcm, ref nullpacketmem);
                        Engine.Packets.Enqueue(new AudioPacket
                        {
                            Bytes = nullpacketmem,
                            IsSilence = true,
                            MillisecondDuration = 20
                        });
                    }
                }
                else if (Engine.Packets.Count == 0)
                {
                    await SendSpeakingAsync(false).ConfigureAwait(false);
                    Volatile.Write(ref Engine.IsPlaybackCompleted, true);
                }
            }
        }
    }
}