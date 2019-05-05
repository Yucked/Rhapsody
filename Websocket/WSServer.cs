using Frostbyte.Attributes;
using Frostbyte.Entities;
using Frostbyte.Entities.Operations;
using Frostbyte.Handlers;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Net;
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

            _statsSenderTask = Task.Run(CollectStatsAsync, _statsCancellation.Token);
            while (!_wsCancellation.IsCancellationRequested)
            {
                var context = await _listener.GetContextAsync().ConfigureAwait(false);
                await ProcessRequestAsync(context).ConfigureAwait(false);
            }
        }

        private async Task ProcessRequestAsync(HttpListenerContext context)
        {
            switch (context.Request.Url.LocalPath)
            {
                case "/loadtracks":
                    if (context.Response.Headers.Get("Password") != _config.Password)
                    {
                        context.Response.StatusCode = 403;
                        context.Response.StatusDescription = "Password header doesn't match config's value.";
                    }


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
            while (!_statsCancellation.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromSeconds(15)).ConfigureAwait(false);
                var process = Process.GetCurrentProcess();

                var stats = new StatisticsOp
                {
                    ConnectedPlayers = _clients.Count,
                    PlayingPlayers = _clients.Values.Sum(x => x.GuildConnections.Count),
                    Uptime = DateTimeOffset.UtcNow - process.StartTime.ToUniversalTime()
                }.Populate(process);

                var serialize = JsonConvert.SerializeObject(stats);
                var sendTasks = _clients.Select(x => x.Value.SendAsync(serialize));
                await Task.WhenAll(sendTasks).ConfigureAwait(false);
            }
        }
    }
}