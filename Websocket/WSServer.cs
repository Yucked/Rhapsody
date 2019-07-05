using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Frostbyte.Entities;
using Frostbyte.Entities.Enums;
using Frostbyte.Entities.Packets;
using Frostbyte.Entities.Responses;
using Frostbyte.Extensions;
using Frostbyte.Handlers;

namespace Frostbyte.Websocket
{
    public sealed class WsServer
    {
        private readonly ConcurrentDictionary<IPEndPoint, WsClient> _clients;
        private readonly Configuration _config;
        private readonly HttpListener _listener;

        private readonly CancellationTokenSource _mainCancellation;
        private readonly SourceHandler _sources;
        private readonly CancellationTokenSource _wsCancellation;
        private CancellationTokenSource _statsCancellation;
        private Task _statsSenderTask;

        public WsServer()
        {
            _listener = new HttpListener();
            _clients = new ConcurrentDictionary<IPEndPoint, WsClient>();
            _wsCancellation = new CancellationTokenSource();
            _statsCancellation = new CancellationTokenSource();
            _mainCancellation =
                CancellationTokenSource.CreateLinkedTokenSource(_wsCancellation.Token, _statsCancellation.Token);
            _config = Singleton.Of<Configuration>();
            _sources = Singleton.Of<SourceHandler>();
        }

        public async Task InitializeAsync()
        {
            LogHandler<WsServer>.Log.Information("TLS11, TLS12 & TLS13 set as security protocol.");
            ServicePointManager.SecurityProtocol |=
                SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13;

            LogHandler<WsServer>.Log.Information($"Checking port {_config.Port} status.");
            var globalProps = IPGlobalProperties.GetIPGlobalProperties();
            var activeListners = globalProps.GetActiveTcpListeners();
            var random = new Random();
            foreach (var listner in activeListners)
                if (listner.Port == _config.Port)
                {
                    LogHandler<WsServer>.Log.Warning($"{_config.Port} is already in use. Picking a random port.");
                    _config.Port = random.Next(IPEndPoint.MinPort, IPEndPoint.MaxPort);
                    Singleton.Update<Configuration>(_config);
                }

            _listener.Prefixes.Add(_config.Url);
            _listener.Start();
            LogHandler<WsServer>.Log.Information($"Server started on {_config.Url}.");

            _statsSenderTask = Task.Run(CollectStatsAsync, _statsCancellation.Token);
            _ = CleanupAsync();

            while (!_wsCancellation.IsCancellationRequested)
            {
                var context = await _listener.GetContextAsync().ConfigureAwait(false);
                _ = ProcessRequestAsync(context).ConfigureAwait(false);
            }
        }

        private async Task ProcessRequestAsync(HttpListenerContext context)
        {
            var localPath = context.Request.Url.LocalPath;
            var remoteEndPoint = context.Request.RemoteEndPoint;
            var response = new BaseResponse();

            try
            {
                switch (localPath)
                {
                    case "/tracks":
                        LogHandler<WsServer>.Log.Debug($"Incoming http request from {remoteEndPoint}.");
                        if (context.Request.Headers.Get("Password") != _config.Password)
                        {
                            response.Error = "Password header doesn't match value specified in configuration";
                        }
                        else
                        {
                            var (prov, query) = context.Request.QueryString.BuildQuery();

                            if (query is null || prov is null)
                            {
                                response.Error =
                                    "Please use the `?prov={provider}&q={YOUR_QUERY} argument after /tracks";
                            }
                            else
                            {
                                var request = await _sources.HandleRequestAsync(prov, query).ConfigureAwait(false);
                                if (request.IsEnabled)
                                    response.Error = $"Requested {prov} isn't enabled in configuration.";
                                else
                                    response.Data = request.Response;
                            }
                        }

                        response.Op = OperationType.Rest;
                        await context.SendResponseAsync(response).ConfigureAwait(false);
                        LogHandler<WsServer>.Log.Debug($"Replied to {remoteEndPoint} with {response.Error}.");
                        break;

                    case "/":
                        if (!context.Request.IsWebSocketRequest)
                        {
                            response.Error =
                                "Only websocket connections are allowed at this endpoint. For rest use /tracks endpoint.";
                            await context.SendResponseAsync(response).ConfigureAwait(false);
                            return;
                        }

                        LogHandler<WsServer>.Log.Debug($"Incoming websocket request coming from {remoteEndPoint}.");

                        var wsContext = await context.AcceptWebSocketAsync(default).ConfigureAwait(false);
                        var wsClient = new WsClient(wsContext);
                        _clients.TryAdd(remoteEndPoint, wsClient);

                        if (_clients.Count > 0 && _statsSenderTask == null)
                        {
                            _statsCancellation = new CancellationTokenSource();
                            _statsSenderTask = CollectStatsAsync();
                        }

                        LogHandler<WsServer>.Log.Information($"Websocket connection opened from {remoteEndPoint}.");
                        break;

                    default:
                        LogHandler<WsServer>.Log.Warning(
                            $"{remoteEndPoint} requested an unknown path: {context.Request.Url}.");
                        response.Error = "You are trying to access an unknown endpoint.";
                        await context.SendResponseAsync(response).ConfigureAwait(false);
                        break;
                }
            }
            catch (Exception ex)
            {
                response.Error = $"Frostbyte threw an inner exception: {ex?.InnerException?.Message ?? ex?.Message}";
                await context.SendResponseAsync(response).ConfigureAwait(false);
                LogHandler<WsServer>.Log.Error(exception: ex);
            }
            finally
            {
                context.Response.Close();
            }
        }

        private async Task CollectStatsAsync()
        {
            while (!_statsCancellation.IsCancellationRequested)
            {
                if (_clients.IsEmpty)
                {
                    await Task.Delay(TimeSpan.FromSeconds(15)).ConfigureAwait(false);
                    continue;
                }

                var process = Process.GetCurrentProcess();

                var stats = new StatisticPacket
                {
                    ConnectedClients = _clients.Count,
                    ConnectedPlayers = _clients.Values.Sum(x => x.VoiceClients.Values.Count(x => x.Engine.IsReady)),
                    PlayingPlayers = _clients.Values.Sum(x => x.VoiceClients.Values.Count(x => x.Engine.IsPlaying)),
                    Uptime = (int) (DateTimeOffset.UtcNow - process.StartTime.ToUniversalTime()).TotalSeconds
                }.Populate(process);

                var rawString = JsonSerializer.ToString(stats);
                LogHandler<StatisticPacket>.Log.Debug(rawString);

                var sendTasks = _clients.Where(x => !x.Value.IsDisposed)
                    .Select(x => x.Value.SendStatsAsync(stats));

                await Task.WhenAll(sendTasks)
                    .ContinueWith(async _ => await Task.Delay(TimeSpan.FromSeconds(30)));
            }
        }

        private async Task CleanupAsync()
        {
            while (!_mainCancellation.IsCancellationRequested)
            {
                if (_clients.Count < 0)
                    await Task.Delay(TimeSpan.FromSeconds(30))
                        .ConfigureAwait(false);

                foreach (var client in _clients)
                    if (client.Value.IsDisposed)
                        _clients.TryRemove(client.Key, out _);

                if (_clients.Count < 0)
                {
                    _statsCancellation?.Cancel(false);
                    _statsCancellation?.Dispose();
                    _statsSenderTask?.Dispose();
                }

                await Task.Delay(5000).ConfigureAwait(false);
            }
        }
    }
}