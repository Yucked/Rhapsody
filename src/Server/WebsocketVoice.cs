using System;
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
    public sealed class WebsocketVoice
    {
        public AudioPlayer Player { get; }

        private readonly CancellationTokenSource _cancellation;
        private readonly ClientWebSocket _socket;
        private readonly UdpClient _udp;
        private readonly ulong _userId;

        private ulong _guildId;
        private Task _heartBeat;
        private CancellationTokenSource _heartBeatCancel;
        private VoiceServerPayload _oldState;
        private VoiceInfo _voiceInfo;

        public WebsocketVoice(ulong userId)
        {
            _userId = userId;
            _udp = new UdpClient();
            _socket = new ClientWebSocket();
            _cancellation = new CancellationTokenSource();
            _heartBeatCancel = new CancellationTokenSource();
            _voiceInfo = new VoiceInfo();

            Player = new AudioPlayer();
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

                var payload = new BaseDiscordPayload(VoiceOpType.Identify,
                    new IdentifyPayload
                    {
                        ServerId = $"{serverPayload.GuildId}",
                        SessionId = serverPayload.SessionId,
                        UserId = $"{_userId}",
                        Token = serverPayload.Token
                    });

                await _socket.SendAsync(payload)
                    .ConfigureAwait(false);

                LogFactory.Debug<WebsocketVoice>($"Sent Identify payload to {serverPayload.GuildId}'s voice ws.");

                _ = ReceiveAsync()
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                switch (ex)
                {
                    case WebSocketException socketException:
                        if (socketException.ErrorCode != 4014 || socketException.ErrorCode != 4015)
                            return;

                        LogFactory.Warning<WebsocketVoice>($"Trying to resume voice connection for {_guildId}.");
                        var payload = new BaseDiscordPayload(VoiceOpType.Resume, new ResumePayload
                        {
                            ServerId = $"{_guildId}",
                            SessionId = _oldState.SessionId,
                            Token = _oldState.Token
                        });
                        await _socket.SendAsync(payload)
                            .ConfigureAwait(false);
                        break;
                }

                LogFactory.Error<WebsocketVoice>(exception: ex);
            }
        }

        private async Task ReceiveAsync()
        {
            LogFactory.Debug<WebsocketVoice>($"Started receive task for {_guildId}.");
            while (!_cancellation.IsCancellationRequested && _socket.State == WebSocketState.Open)
                try
                {
                    var bytes = new byte[256];
                    var memory = new Memory<byte>(bytes);
                    var result = await _socket.ReceiveAsync(memory, default)
                        .ConfigureAwait(false);

                    switch (result.MessageType)
                    {
                        case WebSocketMessageType.Close:
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
                    LogFactory.Error<WebsocketClient>($"{_guildId} voice ws connection closed.", ex);
                }
        }

        private async Task ProcessMessageAsync(ReadOnlyMemory<byte> bytes)
        {
            var payload = bytes.Deserialize<BaseDiscordPayload>();
            switch (payload.Op)
            {
                case VoiceOpType.Ready:
                    var readyPayload = bytes.Deserialize<VoiceReadyPayload>();
                    _udp.Connect(readyPayload.IpAddress, readyPayload.Port);
                    await _udp.SendDiscoveryAsync(readyPayload.Ssrc)
                        .ConfigureAwait(false);

                    LogFactory.Debug<WebsocketVoice>($"Sent UDP discovery for {_guildId}.");

                    var select = new BaseDiscordPayload(VoiceOpType.SelectProtocol,
                        new SelectPayload(readyPayload.IpAddress, readyPayload.Port));
                    await _socket.SendAsync(select)
                        .ConfigureAwait(false);

                    LogFactory.Debug<WebsocketVoice>($"Sent select protcol for {_guildId}.");
                    _voiceInfo.Ssrc = readyPayload.Ssrc;
                    break;

                case VoiceOpType.SessionDescription:
                    var descriptionPayload = bytes.Deserialize<SessionDescriptionPayload>();
                    if (descriptionPayload.Mode != "xsalsa20_poly1305")
                        return;

                    var speakingPayload = new BaseDiscordPayload(VoiceOpType.Speaking,
                        new SpeakingPayload
                        {
                            Delay = 0,
                            IsSpeaking = false,
                            SSRC = _voiceInfo.Ssrc
                        });
                    await _socket.SendAsync(speakingPayload)
                        .ConfigureAwait(false);

                    _ = SendKeepAliveAsync()
                        .ConfigureAwait(false);

                    _voiceInfo.Key = descriptionPayload.SecretKey;
                    break;

                case VoiceOpType.Hello:
                    var helloPayload = bytes.Deserialize<HelloPayload>();
                    if (_heartBeat != null)
                    {
                        _heartBeatCancel.Cancel(false);
                        _heartBeat.Dispose();
                        _heartBeat = null;
                    }

                    _heartBeatCancel = new CancellationTokenSource();
                    _heartBeat = HandleHeartbeatAsync(helloPayload.HeartBeatInterval);
                    break;

                case VoiceOpType.Resumed:
                    LogFactory.Information<WebsocketVoice>($"{_guildId}'s voice ws connection has been resumed.");
                    break;
            }
        }

        private async Task HandleHeartbeatAsync(int interval)
        {
            LogFactory.Debug<WebsocketVoice>($"Started heartbeat task for {_guildId}.");
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
            LogFactory.Debug<WebsocketVoice>($"Started keep alive task for {_guildId}.");
            var keepAlive = 0;
            while (!_cancellation.IsCancellationRequested)
            {
                await _udp.SendKeepAliveAsync(ref keepAlive)
                    .ConfigureAwait(false);
                await Task.Delay(4500, _cancellation.Token)
                    .ConfigureAwait(false);
            }
        }
    }
}