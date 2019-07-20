using System;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Frostbyte.Audio;
using Frostbyte.Entities.Discord;
using Frostbyte.Entities.Enums;
using Frostbyte.Entities.Infos;
using Frostbyte.Entities.Payloads;
using Frostbyte.Factories;
using Frostbyte.Misc;

namespace Frostbyte.Server
{
    public sealed class WebsocketVoice : IAsyncDisposable
    {
        public bool IsConnected
            => Volatile.Read(ref _isConnected);

        public AudioPlayer Player { get; }

        private readonly CancellationTokenSource _cancellation;
        private readonly ConcurrentQueue<AudioPacket> _packets;
        private readonly ClientWebSocket _socket;
        private readonly UdpClient _udp;
        private readonly ulong _userId;

        private ulong _guildId;
        private Task _heartBeat;
        private CancellationTokenSource _heartBeatCancel;
        private bool _isConnected;
        private VoiceServerPayload _oldState;
        private VoiceInfo _voiceInfo;

        public WebsocketVoice(ulong userId)
        {
            _userId = userId;
            _udp = new UdpClient();
            _socket = new ClientWebSocket();
            _cancellation = new CancellationTokenSource();
            _heartBeatCancel = new CancellationTokenSource();
            _packets = new ConcurrentQueue<AudioPacket>();
            _voiceInfo = new VoiceInfo();

            Player = new AudioPlayer();
        }

        public async ValueTask DisposeAsync()
        {
            _cancellation.Cancel(false);
            _heartBeatCancel?.Cancel(false);
            _heartBeat?.Dispose();
            _udp.Close();
            await _socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client disconnected.", CancellationToken.None)
                .ConfigureAwait(false);
            _socket.Dispose();

            LogFactory.Warning<WebsocketVoice>($"Websocket connection for {_guildId} has been disposed.");
            Volatile.Write(ref _isConnected, false);
        }

        public async Task ProcessVoiceServerPaylaodAsync(VoiceServerPayload serverPayload)
        {
            if (_socket != null && _socket.State == WebSocketState.Open && _oldState?.Endpoint != serverPayload.Endpoint)
            {
                await _socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Changing voice server.",
                        CancellationToken.None)
                    .ConfigureAwait(false);

                LogFactory.Information<WebsocketVoice>($"Changing {_guildId} voice server to {serverPayload.Endpoint}.");
            }

            _oldState = serverPayload;
            _guildId = _oldState.GuildId;

            try
            {
                LogFactory.Debug<WebsocketVoice>($"Starting voice ws connection to Discord for {serverPayload.GuildId} guild.");

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
                    await _udp.SendDiscoveryAsync(readyData.Ssrc)
                        .ConfigureAwait(false);

                    LogFactory.Debug<WebsocketVoice>($"Sent UDP discovery for guild {_guildId}.");

                    var select = new BaseDiscordPayload<SelectPayload>(
                        new SelectPayload(readyData.IpAddress, readyData.Port));
                    await _socket.SendAsync(select)
                        .ConfigureAwait(false);

                    LogFactory.Debug<WebsocketVoice>($"Sent select protocol for guild {_guildId}.");
                    _voiceInfo.Ssrc = readyData.Ssrc;
                    break;

                case VoiceOpType.SessionDescription:
                    var sessionPayload = bytes.Deserialize<BaseDiscordPayload<SessionData>>();
                    var sessionData = sessionPayload.Data;

                    if (sessionData.Mode != "xsalsa20_poly1305")
                        return;

                    var speakingPayload = new BaseDiscordPayload<SpeakingData>(
                        new SpeakingData
                        {
                            Delay = 0,
                            IsSpeaking = false,
                            SSRC = _voiceInfo.Ssrc
                        });
                    await _socket.SendAsync(speakingPayload)
                        .ConfigureAwait(false);

                    _ = SendKeepAliveAsync()
                        .ConfigureAwait(false);

                    _voiceInfo.Key = sessionData.Secret.ConvertToByte();
                    break;

                case VoiceOpType.Hello:
                    var helloPayload = bytes.Deserialize<BaseDiscordPayload<HelloData>>();
                    var helloData = helloPayload.Data;

                    if (_heartBeat != null)
                    {
                        _heartBeatCancel.Cancel(false);
                        _heartBeat.Dispose();
                        _heartBeat = null;
                    }

                    _heartBeatCancel = new CancellationTokenSource();
                    _heartBeat = HandleHeartbeatAsync(helloData.Interval);
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
            var keepAlive = 0;
            while (!_cancellation.IsCancellationRequested)
            {
                await _udp.SendKeepAliveAsync(ref keepAlive)
                    .ConfigureAwait(false);
                await Task.Delay(4500, _cancellation.Token)
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
                        LogFactory.Warning<WebsocketVoice>($"Discord closed ws connection for {_guildId}. Try reconnecting?");
                        await DisposeAsync()
                            .ConfigureAwait(false);
                        break;
                }

            LogFactory.Error<WebsocketVoice>(exception: exception);
        }
    }
}