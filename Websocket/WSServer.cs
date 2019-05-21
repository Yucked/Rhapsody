using Frostbyte.Attributes;
using Frostbyte.Entities;
using Frostbyte.Entities.Operations;
using Frostbyte.Handlers;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Frostbyte.Extensions;

namespace Frostbyte.Websocket
{
    [Service(ServiceLifetime.Singleton, typeof(StatsHandler))]
    public sealed class WsServer : IAsyncDisposable
    {
        private readonly ConcurrentDictionary<ulong, WsClient> _clients;
        private readonly HttpListener _listener;
        private readonly LogHandler<WsServer> _log;
        private readonly CancellationTokenSource _mainCancellation, _wsCancellation, _statsCancellation;
        private readonly ConcurrentDictionary<ulong, CancellationTokenSource> _receiveTokens;

        private ConfigEntity _config;
        private CancellationTokenSource _receiveCancellation;
        private StatsHandler _statsHandler;
        private Task _statsSenderTask;

        public WsServer()
        {
            _listener = new HttpListener();
            _log = new LogHandler<WsServer>();
            _clients = new ConcurrentDictionary<ulong, WsClient>();
            _receiveTokens = new ConcurrentDictionary<ulong, CancellationTokenSource>();
            _wsCancellation = new CancellationTokenSource();
            _statsCancellation = new CancellationTokenSource();
            _mainCancellation = CancellationTokenSource.CreateLinkedTokenSource(_wsCancellation.Token, _statsCancellation.Token);
        }

        public async ValueTask DisposeAsync()
        {
            foreach (var (_, token) in _receiveTokens)
            {
                token.Cancel(false);
            }

            foreach (var client in _clients)
            {
                await client.Value.DisposeAsync();
            }

            _mainCancellation.Cancel(false);
            _listener.Close();
            _clients.Clear();
            _receiveTokens.Clear();
        }

        public async Task InitializeAsync(ConfigEntity config)
        {
            _config = config;
            _log.LogInformation("Security protocol set to TLS11 & TLS12.");
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

            _listener.Prefixes.Add(config.Url);
            _listener.Start();
            _log.LogInformation($"HTTP listener listening on: {config.Url}.");

            //_statsSenderTask = Task.Run(CollectStatsAsync, _statsCancellation.Token);
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
            var response = new ResponseEntity();

            switch (localPath)
            {
                case "/loadtracks":

                    if (context.Request.Headers.Get("Password") != _config.Password)
                    {
                        response.IsSuccess = false;
                        response.Reason = "Password header doesn't match value specified in configuration.";
                        await context.SendResponseAsync(response).ConfigureAwait(false);
                    }
                    else
                    {
                        response.IsSuccess = true;
                        response.Reason = "Password match was a success!";
                        await context.SendResponseAsync(response).ConfigureAwait(false);
                    }

                    _log.LogDebug($"Processed REST request for {localPath} path from {remoteEndPoint}.");
                    context.Response.Close();
                    break;

                case "/":

                    if (!context.Request.IsWebSocketRequest)
                    {
                        response.Reason = "Only websocket connections are allowed at this endpoint.";
                        response.IsSuccess = false;
                        await context.SendResponseAsync(response).ConfigureAwait(false);
                        context.Response.Close();

                        _log.LogDebug($"Returned 400 for {localPath} path from {remoteEndPoint}.");
                        return;
                    }

                    _log.LogDebug($"Websocket connection opened from {remoteEndPoint}.");

                    var ws = await context.AcceptWebSocketAsync(default).ConfigureAwait(false);
                    ulong.TryParse(ws.Headers.Get("User-Id"), out var userId);
                    int.TryParse(ws.Headers.Get("Shards"), out var shards);

                    var wsClient = new WsClient(ws, userId, shards, remoteEndPoint);
                    wsClient.OnClosed += OnClosed;
                    _clients.TryAdd(userId, wsClient);

                    _receiveCancellation = new CancellationTokenSource();
                    var receive = wsClient.ReceiveAsync(_receiveCancellation);
                    _receiveTokens.TryAdd(userId, _receiveCancellation);

                    await wsClient.SendAsync("Foo Bar");

                    _log.LogInformation($"Websocket connected from {remoteEndPoint} with {userId} id.");
                    break;

                default:
                    _log.LogWarning($"{remoteEndPoint} requested an unknown path: {context.Request.Url}.");
                    break;
            }
        }

        private Task OnClosed(IPEndPoint endPoint, ulong userId)
        {
            _clients.TryRemove(userId, out _);
            _receiveTokens.TryRemove(userId, out var token);
            token.Cancel(false);

            _log.LogWarning($"Client {endPoint} closed websocket connection.");
            return default;
        }

        private async Task CollectStatsAsync()
        {
            _log.LogDebug("Entering sending statistics loop.");
            while (!_statsCancellation.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromSeconds(15)).ConfigureAwait(false);
                var process = Process.GetCurrentProcess();

                var stats = new StatisticsOp
                {
                    ConnectedPlayers = _clients.Count,
                    PlayingPlayers = _clients.Values.Sum(x => x.GuildConnections.Count),
                    Uptime = DateTimeOffset.UtcNow - process.StartTime.ToUniversalTime()
                };

                var sendTasks = _clients.Select(x => x.Value.SendAsync(stats));
                await Task.WhenAll(sendTasks).ConfigureAwait(false);
            }
        }
    }
}