using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Frostbyte.Audio;
using Frostbyte.Entities.Discord;
using Frostbyte.Entities.Enums;
using Frostbyte.Entities.Payloads;
using Frostbyte.Factories;

namespace Frostbyte.Server
{
    public sealed class WebsocketVoice : IAsyncDisposable
    {
        public bool IsConnected
            => Volatile.Read(ref _isConnected);

        public AudioPlayer Player { get; }
        public AudioStream AudioStream { get; }
        private readonly Task _audioSenderTask;

        private readonly CancellationTokenSource _cancellation;
        private readonly ClientWebSocket _socket;
        private readonly UdpClient _udp;
        private readonly ulong _userId;
        private ulong _guildId;
        private CancellationTokenSource _heartBeatCancel;
        private Task _heartBeatTask;
        private bool _isConnected;
        private ReadOnlyMemory<byte> _key;
        private VoiceServerPayload _oldState;
        private uint _ssrc;

        public WebsocketVoice(ulong userId)
        {
            _userId = userId;
            _udp = new UdpClient();
            _socket = new ClientWebSocket();
            _cancellation = new CancellationTokenSource();
            _heartBeatCancel = new CancellationTokenSource();
            AudioStream = new AudioStream(20);
            Player = new AudioPlayer();

            _audioSenderTask = SendAudioAsync();
        }

        public async ValueTask DisposeAsync()
        {
            _cancellation.Cancel(false);
            _heartBeatCancel?.Cancel(false);
            _heartBeatTask?.Dispose();
            _audioSenderTask?.Dispose();
            _udp.Close();
            await _socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client disconnected.", CancellationToken.None)
                .ConfigureAwait(false);
            _socket.Dispose();

            LogFactory.Warning<WebsocketVoice>($"Websocket connection for {_guildId} has been disposed.");
            Volatile.Write(ref _isConnected, false);
        }

        public async Task ProcessVoiceServerPaylaodAsync(VoiceServerPayload serverPayload)
        {
            if (_socket != null && _socket.State == WebSocketState.Open &&
                _oldState?.Endpoint != serverPayload.Endpoint)
            {
                await _socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Changing voice server.",
                        CancellationToken.None)
                    .ConfigureAwait(false);

                LogFactory.Information<WebsocketVoice>(
                    $"Changing {_guildId} voice server to {serverPayload.Endpoint}.");
            }

            _oldState = serverPayload;
            _guildId = _oldState.GuildId;

            try
            {
                LogFactory.Debug<WebsocketVoice>(
                    $"Starting voice ws connection to Discord for {serverPayload.GuildId} guild.");

                var url = $"wss://{serverPayload.Endpoint.Sub(0, serverPayload.Endpoint.Length - 3)}"
                    .WithParameter("encoding", "json")
                    .WithParameter("v", "3")
                    .ToUrl();

                await _socket.ConnectAsync(url, _cancellation.Token)
                    .ConfigureAwait(false);

                LogFactory.Debug<WebsocketVoice>($"{serverPayload.GuildId} guild's voice ws connection established.");

                var payload = new BaseDiscordPayload<IdentifyData>(
                    new IdentifyData
                    {
                        ServerId = $"{serverPayload.GuildId}",
                        SessionId = serverPayload.SessionId,
                        UserId = $"{_userId}",
                        Token = serverPayload.Token
                    });

                await _socket.SendAsync(payload)
                    .ConfigureAwait(false);

                LogFactory.Debug<WebsocketVoice>($"Sent Identify payload for guild {serverPayload.GuildId}.");

                _ = ReceiveAsync()
                    .ConfigureAwait(false);
                Volatile.Write(ref _isConnected, true);
            }
            catch (Exception ex)
            {
                await VerifyExceptionAsync(ex)
                    .ConfigureAwait(false);
            }
        }

        private async Task ReceiveAsync()
        {
            LogFactory.Debug<WebsocketVoice>($"Now receiving ws data for {_guildId} guild.");
            while (!_cancellation.IsCancellationRequested && _socket.State == WebSocketState.Open)
                try
                {
                    var bytes = new byte[384];
                    var memory = new Memory<byte>(bytes);
                    var result = await _socket.ReceiveAsync(memory, default)
                        .ConfigureAwait(false);

                    switch (result.MessageType)
                    {
                        case WebSocketMessageType.Close:
                            await DisposeAsync()
                                .ConfigureAwait(false);
                            break;

                        case WebSocketMessageType.Text:
                            if (!result.EndOfMessage)
                                continue;

                            Extensions.TrimEnd(ref bytes);
                            await ProcessMessageAsync(bytes)
                                .ConfigureAwait(false);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    await VerifyExceptionAsync(ex)
                        .ConfigureAwait(false);
                }
        }

        private async Task ProcessMessageAsync(ReadOnlyMemory<byte> bytes)
        {
            // Ignore Heartbeat ACK
            if (bytes.Span[6] != '6')
            {
                var raw = bytes.GetString();
                LogFactory.Debug<WebsocketVoice>(raw);
            }

            var payload = bytes.Deserialize<BaseDiscordPayload<object>>();
            switch (payload.Op)
            {
                case VoiceOpType.Ready:
                    var readyPayload = bytes.Deserialize<BaseDiscordPayload<ReadyData>>();
                    var readyData = readyPayload.Data;

                    _udp.Connect(readyData.IpAddress, readyData.Port);
                    await _udp.SendSsrcAsync(readyData.Ssrc)
                        .ConfigureAwait(false);

                    LogFactory.Debug<WebsocketVoice>($"Sent UDP discovery for guild {_guildId}.");

                    var select = new BaseDiscordPayload<SelectPayload>(
                        new SelectPayload(readyData.IpAddress, readyData.Port));
                    await _socket.SendAsync(select)
                        .ConfigureAwait(false);

                    LogFactory.Debug<WebsocketVoice>($"Sent select protocol for guild {_guildId}.");
                    _ssrc = readyData.Ssrc;
                    AudioStream.SetSsrc(readyData.Ssrc);
                    break;

                case VoiceOpType.SessionDescription:
                    var sessionPayload = bytes.Deserialize<BaseDiscordPayload<SessionData>>();
                    var sessionData = sessionPayload.Data;

                    if (sessionData.Mode != "xsalsa20_poly1305")
                        return;

                    AudioStream.SetKey(sessionData.Secret);
                    _key = sessionData.Secret.ConvertToByte();

                    await SetSpeakingAsync(false)
                        .ConfigureAwait(false);

                    QueueNullPackets();

                    _ = SendKeepAliveAsync()
                        .ConfigureAwait(false);
                    break;

                case VoiceOpType.Hello:
                    var helloPayload = bytes.Deserialize<BaseDiscordPayload<HelloData>>();
                    var helloData = helloPayload.Data;

                    if (_heartBeatTask != null)
                    {
                        _heartBeatCancel.Cancel(false);
                        _heartBeatTask.Dispose();
                        _heartBeatTask = null;
                    }

                    _heartBeatCancel = new CancellationTokenSource();
                    _heartBeatTask = HandleHeartbeatAsync(helloData.Interval);
                    break;

                case VoiceOpType.Resumed:
                    Volatile.Write(ref _isConnected, true);
                    LogFactory.Information<WebsocketVoice>($"Guild {_guildId} voice ws connection has been resumed.");
                    break;
            }
        }

        private async Task HandleHeartbeatAsync(int interval)
        {
            LogFactory.Debug<WebsocketVoice>($"Started heartbeat task for guild {_guildId}.");
            while (!_heartBeatCancel.IsCancellationRequested)
            {
                var payload = new BaseDiscordPayload<long>(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
                await _socket.SendAsync(payload)
                    .ConfigureAwait(false);
                await Task.Delay(interval, _heartBeatCancel.Token)
                    .ConfigureAwait(false);
            }
        }

        private async Task SendKeepAliveAsync()
        {
            LogFactory.Debug<WebsocketVoice>($"Started keep alive task for guild {_guildId}.");
            var keepAlive = (ulong) 0;
            while (!_cancellation.IsCancellationRequested)
            {
                await _udp.SendKeepAliveAsync(ref keepAlive)
                    .ConfigureAwait(false);
                await Task.Delay(5000, _cancellation.Token)
                    .ConfigureAwait(false);
            }
        }

        private async Task VerifyExceptionAsync(Exception exception)
        {
            if (exception is WebSocketException socketException)
                switch (socketException.ErrorCode)
                {
                    case 4014:
                    case 4015:

                        LogFactory.Warning<WebsocketVoice>($"Trying to resume voice connection for {_guildId} guild.");
                        var payload = new BaseDiscordPayload<ResumeData>(new ResumeData
                        {
                            ServerId = $"{_guildId}",
                            SessionId = _oldState.SessionId,
                            Token = _oldState.Token
                        });

                        await _socket.SendAsync(payload)
                            .ConfigureAwait(false);
                        break;

                    case 10054:
                        LogFactory.Warning<WebsocketVoice>(
                            $"Discord closed ws connection for {_guildId}. Try reconnecting?");
                        await DisposeAsync()
                            .ConfigureAwait(false);
                        break;
                }

            LogFactory.Error<WebsocketVoice>(exception: exception);
        }

        private async Task SetSpeakingAsync(bool isSpeaking)
        {
            var speakingPayload = new BaseDiscordPayload<SpeakingData>(
                new SpeakingData
                {
                    Delay = 0,
                    IsSpeaking = isSpeaking,
                    Ssrc = _ssrc
                });

            await _socket.SendAsync(speakingPayload)
                .ConfigureAwait(false);
        }

        private void QueueNullPackets(bool isSilence = false)
        {
            var nullpcm = new byte[AudioHelper.GetSampleSize(20)];
            for (var i = 0; i < 3; i++)
            {
                var opus = new byte[nullpcm.Length];
                var opusMem = opus.AsMemory();
                AudioHelper.PrepareAudioPacket(nullpcm, ref opusMem, _ssrc, _key);
                AudioStream.Packets.Enqueue(new AudioPacket(opusMem, 20, isSilence));
            }
        }

        private async Task SendAudioAsync()
        {
            var syncTicks = (double) Stopwatch.GetTimestamp();
            var syncRes = Stopwatch.Frequency * 0.005;
            var tickRes = 10_000_000.0 / Stopwatch.Frequency;

            while (!_cancellation.IsCancellationRequested)
            {
                var hasPacket = AudioStream.Packets.TryDequeue(out var packet);

                if (hasPacket && Player.State == PlayerState.Null)
                    Player.State = PlayerState.Playing;

                var durationModifier = hasPacket ? packet.MsDuration / 5 : 4;
                var cts = Math.Max(Stopwatch.GetTimestamp() - syncTicks, 0);
                if (cts < syncRes * durationModifier)
                {
                    var delay = TimeSpan.FromTicks((long) ((syncRes * durationModifier - cts) * tickRes));
                    await Task.Delay(delay)
                        .ConfigureAwait(false);
                }

                syncTicks += syncRes * durationModifier;

                if (!hasPacket || Player.State == PlayerState.Paused)
                    continue;

                await SetSpeakingAsync(true)
                    .ConfigureAwait(false);

                await _udp.SendAsync(packet.Bytes)
                    .ConfigureAwait(false);

                if (!packet.IsSilence && AudioStream.Packets.Count == 0)
                    QueueNullPackets(true);
                else if (AudioStream.Packets.Count == 0)
                {
                    await SetSpeakingAsync(false)
                        .ConfigureAwait(false);
                    Player.State = PlayerState.Stopped;
                }
            }
        }
    }
}