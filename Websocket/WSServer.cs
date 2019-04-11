using Frostbyte.Attributes;
using Frostbyte.Entities;
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
    public sealed class WSServer : IAsyncDisposable
    {
        private readonly HttpListener _listener;
        private readonly LogHandler<WSServer> _log;
        private readonly ConcurrentDictionary<ulong, WSClient> _clients;
        private readonly ConcurrentDictionary<CancellationTokenSource, Task> _receiveTasks;

        private ConfigEntity config;
        private Task StatsSenderTask;
        private StatsHandler statsHandler;
        private CancellationTokenSource MainCancellation, WSCancellation, StatsCancellation, ReceiveCancellation;

        public WSServer()
        {
            _listener = new HttpListener();
            _log = new LogHandler<WSServer>();
            _clients = new ConcurrentDictionary<ulong, WSClient>();
            _receiveTasks = new ConcurrentDictionary<CancellationTokenSource, Task>();
            WSCancellation = new CancellationTokenSource();
            StatsCancellation = new CancellationTokenSource();
            MainCancellation = CancellationTokenSource.CreateLinkedTokenSource(WSCancellation.Token, StatsCancellation.Token);
        }

        public async Task InitializeAsync(ConfigEntity config)
        {
            this.config = config;
            _log.LogInformation($"Security protocol set to TLS11 & TLS12.");
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

            _listener.Prefixes.Add(config.Url);
            _listener.Start();
            _log.LogInformation($"HTTP listner listening on: {config.Url}.");

            StatsSenderTask = Task.Run(CollectStatsAsync, StatsCancellation.Token);
            while (!WSCancellation.IsCancellationRequested)
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
                    if (context.Response.Headers.Get("Password") != config.Password)
                    {
                        context.Response.StatusCode = 403;
                        context.Response.StatusDescription = "Password header doesn't match config's value.";
                        return;
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

                    var wsClient = new WSClient(ws, userId, shards);
                    wsClient.OnClosed += OnCLosed;
                    _clients.TryAdd(userId, wsClient);

                    ReceiveCancellation = new CancellationTokenSource();
                    var receieve = wsClient.ReceiveAsync(ReceiveCancellation);
                    _receiveTasks.TryAdd(ReceiveCancellation, receieve);

                    _log.LogInformation($"Websocket client connected with {userId} id.");
                    break;

                default:
                    _log.LogWarning($"Unknown path requested: {context.Request.Url}.");
                    break;
            }
        }

        private Task OnCLosed(ulong guildId)
        {
            _clients.TryRemove(guildId, out _);
            return Task.CompletedTask;
        }

        private async Task CollectStatsAsync()
        {
            while (!StatsCancellation.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromSeconds(15)).ConfigureAwait(false);
                var process = Process.GetCurrentProcess();

                var stats = new StatisticsEntity
                {
                    ConnectedPlayers = _clients.Count,
                    PlayingPlayers = _clients.Values.Sum(x => x._guildConnections.Count),
                    Uptime = DateTimeOffset.UtcNow - process.StartTime.ToUniversalTime()
                }
                .Populate(process);

                var serialize = JsonConvert.SerializeObject(stats);
                var sendTasks = _clients.Select(x => x.Value.SendAsync(serialize));
                await Task.WhenAll(sendTasks).ConfigureAwait(false);
            }
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

            MainCancellation.Cancel(false);
            _listener.Close();

            _clients.Clear();
        }
    }
}