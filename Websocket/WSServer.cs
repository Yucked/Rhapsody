using Frostbyte.Entities;
using Frostbyte.Handlers;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Frostbyte.Entities.Packets;
using Frostbyte.Extensions;
using System.Text.Json;
using Frostbyte.Entities.Enums;
using System.Net.NetworkInformation;

namespace Frostbyte.Websocket
{
    public sealed class WSServer : IAsyncDisposable
    {
        private readonly Configuration _config;
        private readonly SourceHandler _sources;
        private readonly ConcurrentDictionary<IPEndPoint, WSClient> _clients;
        private readonly HttpListener _listener;
        private readonly CancellationTokenSource _mainCancellation, _wsCancellation, _statsCancellation;

        private CancellationTokenSource _receiveCancellation;
        private Task _statsSenderTask;

        public WSServer()
        {
            _listener = new HttpListener();
            _clients = new ConcurrentDictionary<IPEndPoint, WSClient>();
            _wsCancellation = new CancellationTokenSource();
            _statsCancellation = new CancellationTokenSource();
            _mainCancellation = CancellationTokenSource.CreateLinkedTokenSource(_wsCancellation.Token, _statsCancellation.Token);
            _config = Singleton.Of<Configuration>();
            _sources = Singleton.Of<SourceHandler>();
        }

        public async ValueTask DisposeAsync()
        {
            foreach (var client in _clients)
            {
                await client.Value.DisposeAsync();
            }

            _mainCancellation.Cancel(false);
            _listener.Close();
            _clients.Clear();
            _statsSenderTask.Dispose();
        }

        public async Task InitializeAsync()
        {
            LogHandler<WSServer>.Log.Information("TLS11, TLS12 & TLS13 set as security protocol.");
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13;

            LogHandler<WSServer>.Log.Information($"Checking port {_config.Port} status.");
            var globalProps = IPGlobalProperties.GetIPGlobalProperties();
            var activeListners = globalProps.GetActiveTcpListeners();
            var random = new Random();
            foreach (var listner in activeListners)
            {
                if (listner.Port == _config.Port)
                {
                    LogHandler<WSServer>.Log.Warning($"{_config.Port} is already in use. Picking a random port.");
                    _config.Port = random.Next(IPEndPoint.MinPort, IPEndPoint.MaxPort);
                    Singleton.Update<Configuration>(_config);
                }
            }

            _listener.Prefixes.Add(_config.Url);
            _listener.Start();
            LogHandler<WSServer>.Log.Information($"Server started on {_config.Url}.");

            _statsSenderTask = Task.Run(CollectStatsAsync, _statsCancellation.Token);
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
            var response = new ResponseEntity(false, string.Empty);

            try
            {
                switch (localPath)
                {
                    case "/tracks":
                        LogHandler<WSServer>.Log.Debug($"Incoming http request from {remoteEndPoint}.");
                        if (context.Request.Headers.Get("Password") != _config.Password)
                        {
                            response.Reason = "Password header doesn't match value specified in configuration";
                        }
                        else
                        {
                            var (prov, query) = context.Request.QueryString.BuildQuery();

                            if (query is null || prov is null)
                            {
                                response.Reason = "Please use the `?prov={provider}&q={YOUR_QUERY} argument after /tracks";
                            }
                            else
                            {
                                response.IsSuccess = true;
                                response = await _sources.HandleRequestAsync(prov, query).ConfigureAwait(false);
                            }
                        }

                        response.Operation = OperationType.REST;
                        await context.SendResponseAsync(response).ConfigureAwait(false);
                        LogHandler<WSServer>.Log.Debug($"Replied to {remoteEndPoint} with {response.Reason}.");
                        break;

                    case "/":
                        if (!context.Request.IsWebSocketRequest)
                        {
                            response.Reason = "Only websocket connections are allowed at this endpoint. For rest use /tracks endpoint.";
                            await context.SendResponseAsync(response).ConfigureAwait(false);
                            return;
                        }

                        LogHandler<WSServer>.Log.Debug($"Incoming websocket request coming from {remoteEndPoint}.");

                        var wsContext = await context.AcceptWebSocketAsync(default).ConfigureAwait(false);
                        var wsClient = new WSClient(wsContext, remoteEndPoint);
                        wsClient.OnClosed += OnClosed;
                        _clients.TryAdd(remoteEndPoint, wsClient);
                        _ = wsClient.ReceiveAsync(_receiveCancellation);

                        LogHandler<WSServer>.Log.Information($"Websocket connection opened from {remoteEndPoint}.");
                        break;

                    default:
                        LogHandler<WSServer>.Log.Warning($"{remoteEndPoint} requested an unknown path: {context.Request.Url}.");
                        response.Reason = "You are trying to access an unknown endpoint.";
                        await context.SendResponseAsync(response).ConfigureAwait(false);
                        break;
                }
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Reason = $"Frostbyte threw an inner exception: {ex?.InnerException?.Message ?? ex?.Message}";
                await context.SendResponseAsync(response).ConfigureAwait(false);
                LogHandler<WSServer>.Log.Error(ex);
            }
            finally
            {
                context.Response.Close();
            }
        }

        private Task OnClosed(IPEndPoint endPoint)
        {
            _clients.TryRemove(endPoint, out _);
            return Task.CompletedTask;
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

                var stat = new StatisticPacket
                {
                    ConnectedClients = _clients.Count,
                    ConnectedPlayers = _clients.Values.Sum(x => x.Engines.Values.Count(x => x.IsReady is true)),
                    PlayingPlayers = _clients.Values.Sum(x => x.Engines.Values.Count(e => e.IsPlaying is true)),
                    Uptime = (DateTimeOffset.UtcNow - process.StartTime.ToUniversalTime()).TotalSeconds.TryCast<int>()
                }.Populate(process);

                var rawString = JsonSerializer.ToString(stat);
                LogHandler<StatisticPacket>.Log.Debug(rawString);
                var sendTasks = _clients.Select(x => x.Value._socket.SendAsync(stat).AsTask());
                await Task.WhenAll(sendTasks);
                await Task.Delay(TimeSpan.FromSeconds(30));
            }
        }
    }
}