using Frostbyte.Attributes;
using Frostbyte.Entities;
using Frostbyte.Entities.Operations;
using Frostbyte.Handlers;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace Frostbyte.Websocket
{
    [Service(ServiceLifetime.Singleton, typeof(StatsHandler))]
    public sealed class WsServer : IAsyncDisposable
    {
        private readonly ConcurrentDictionary<ulong, WsClient> _clients;
        private readonly HttpListener _listener;
        private readonly LogHandler<WsServer> _log;
        private readonly CancellationTokenSource _mainCancellation, _wsCancellation, _statsCancellation;
        private readonly ConcurrentDictionary<CancellationTokenSource, Task> _receiveTasks;

        private ConfigEntity _config;
        private CancellationTokenSource _receiveCancellation;
        private StatsHandler _statsHandler;
        private Task _statsSenderTask;

        public WsServer()
        {
            _listener = new HttpListener();
            _log = new LogHandler<WsServer>();
            _clients = new ConcurrentDictionary<ulong, WsClient>();
            _receiveTasks = new ConcurrentDictionary<CancellationTokenSource, Task>();
            _wsCancellation = new CancellationTokenSource();
            _statsCancellation = new CancellationTokenSource();
            _mainCancellation = CancellationTokenSource.CreateLinkedTokenSource(_wsCancellation.Token, _statsCancellation.Token);
        }

        public async ValueTask DisposeAsync()
        {
            foreach (var client in _clients)
                await client.Value.DisposeAsync();

            foreach (var (token, task) in _receiveTasks)
            {
                token.Cancel(false);
                task.Dispose();
            }

            _mainCancellation.Cancel(false);
            _listener.Close();
            _clients.Clear();
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
            _log.LogDebug($"Processing request from {localPath} path.");
            var response = new ResponseEntity();

            switch (localPath)
            {
                case "/loadtracks":

                    if (context.Request.Headers.Get("Password") != _config.Password)
                    {
                        response.IsSuccess = false;
                        response.Reason = "Password header doesn't match value specified in configuration.";
                        await context.Response.OutputStream.WriteAsync(JsonSerializer.ToBytes(response)).ConfigureAwait(false);
                        _log.LogDebug($"Returned 403 for {localPath} path from {context.Request.UserAgent} agent.");
                    }
                    else
                    {

                        response.IsSuccess = true;
                        response.Reason = "Password match was a success!";
                        await context.Response.OutputStream.WriteAsync(JsonSerializer.ToBytes(response)).ConfigureAwait(false);
                    }

                    context.Response.Close();
                    break;

                case "/":
                    if (!context.Request.IsWebSocketRequest)
                    {
                        context.Response.StatusCode = 400;
                        context.Response.Close();
                        return;
                    }

                    var ws = await context.AcceptWebSocketAsync(string.Empty).ConfigureAwait(false);
                    ulong.TryParse(ws.Headers.Get("User-Id"), out var userId);
                    int.TryParse(ws.Headers.Get("Shards"), out var shards);

                    var wsClient = new WsClient(ws, userId, shards);
                    wsClient.OnClosed += OnClosed;
                    _clients.TryAdd(userId, wsClient);

                    _receiveCancellation = new CancellationTokenSource();
                    var receive = wsClient.ReceiveAsync(_receiveCancellation);
                    _receiveTasks.TryAdd(_receiveCancellation, receive);

                    _log.LogInformation($"Websocket client connected with {userId} id.");
                    break;

                default:
                    _log.LogWarning($"Unknown path requested: {context.Request.Url}.");
                    break;
            }
        }

        private Task OnClosed(ulong guildId)
        {
            _clients.TryRemove(guildId, out _);
            return Task.CompletedTask;
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

                var serialize = JsonSerializer.ToBytes(stats);
                var sendTasks = _clients.Select(x => x.Value.SendAsync(serialize));
                await Task.WhenAll(sendTasks).ConfigureAwait(false);
            }
        }
    }
}