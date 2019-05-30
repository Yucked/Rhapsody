using Frostbyte.Attributes;
using Frostbyte.Entities;
using Frostbyte.Handlers;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Frostbyte.Entities.Packets;
using Frostbyte.Extensions;
using System.Text.Json.Serialization;

namespace Frostbyte.Websocket
{
    [Service(ServiceLifetime.Singleton)]
    public sealed class WsServer : IAsyncDisposable
    {
        private readonly ConcurrentDictionary<ulong, WsClient> _clients;
        private readonly HttpListener _listener;
        private readonly CancellationTokenSource _mainCancellation, _wsCancellation, _statsCancellation;
        private readonly ConcurrentDictionary<ulong, CancellationTokenSource> _receiveTokens;
        private readonly SourceHandler _sourceHandler;

        private ConfigEntity _config;
        private CancellationTokenSource _receiveCancellation;
        private Task _statsSenderTask;

        public WsServer(SourceHandler sourceHandler)
        {
            _listener = new HttpListener();
            _clients = new ConcurrentDictionary<ulong, WsClient>();
            _receiveTokens = new ConcurrentDictionary<ulong, CancellationTokenSource>();
            _wsCancellation = new CancellationTokenSource();
            _statsCancellation = new CancellationTokenSource();
            _mainCancellation = CancellationTokenSource.CreateLinkedTokenSource(_wsCancellation.Token, _statsCancellation.Token);
            _sourceHandler = sourceHandler;
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
            _statsSenderTask.Dispose();
        }

        public async Task InitializeAsync(ConfigEntity config)
        {
            _config = config;
            LogHandler<WsServer>.Instance.LogInformation("Security protocol set to TLS11 & TLS12.");
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

            _listener.Prefixes.Add(config.Url);
            _listener.Start();
            LogHandler<WsServer>.Instance.LogInformation($"Server started on {config.Url}.");

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

            switch (localPath)
            {
                case "/tracks":
                    if (context.Request.Headers.Get("Password") != _config.Password)
                    {
                        response.Reason = "Password header doesn't match value specified in configuration.";
                        await context.SendResponseAsync(response).ConfigureAwait(false);
                    }
                    else
                    {
                        var query = context.Request.QueryString.Get("query");
                        if (query is null)
                        {
                            response.Reason = "Please use the `?query={id}:{YOUR_QUERY} argument after /tracks.";
                            await context.SendResponseAsync(response).ConfigureAwait(false);
                        }
                        else
                        {
                            response = await _sourceHandler.HandlerRequestAsync(query).ConfigureAwait(false);
                            await context.SendResponseAsync(response).ConfigureAwait(false);
                        }
                    }

                    context.Response.Close();
                    break;

                case "/":
                    if (!context.Request.IsWebSocketRequest)
                    {
                        response.Reason = "Only websocket connections are allowed at this endpoint. For rest use /tracks endpoint.";
                        await context.SendResponseAsync(response).ConfigureAwait(false);
                        context.Response.Close();
                        return;
                    }

                    LogHandler<WsServer>.Instance.LogDebug($"Incoming websocket request coming from {remoteEndPoint}.");

                    var ws = await context.AcceptWebSocketAsync(default).ConfigureAwait(false);
                    ulong.TryParse(ws.Headers.Get("User-Id"), out var userId);
                    int.TryParse(ws.Headers.Get("Shards"), out var shards);

                    var wsClient = new WsClient(ws, userId, shards, remoteEndPoint);
                    wsClient.OnClosed += OnClosed;
                    _clients.TryAdd(userId, wsClient);

                    _receiveCancellation = new CancellationTokenSource();
                    _ = wsClient.ReceiveAsync(_receiveCancellation);
                    _receiveTokens.TryAdd(userId, _receiveCancellation);

                    LogHandler<WsServer>.Instance.LogInformation($"Websocket connected opened from {remoteEndPoint}.");
                    break;

                default:
                    LogHandler<WsServer>.Instance.LogWarning($"{remoteEndPoint} requested an unknown path: {context.Request.Url}.");
                    response.Reason = "You are trying to access an unknown endpoint.";
                    await context.SendResponseAsync(response).ConfigureAwait(false);
                    context.Response.Close();
                    break;
            }
        }

        private Task OnClosed(IPEndPoint endPoint, ulong userId)
        {
            _clients.TryRemove(userId, out _);
            _receiveTokens.TryRemove(userId, out var token);
            token.Cancel(false);

            LogHandler<WsServer>.Instance.LogWarning($"Client {endPoint} closed websocket connection.");
            return default;
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
                    ConnectedPlayers = _clients.Count,
                    PlayingPlayers = _clients.Values.Sum(x => x.Guilds.Count),
                    Uptime = (int) (DateTimeOffset.UtcNow - process.StartTime.ToUniversalTime()).TotalSeconds
                }.Populate(process);

                var bytes = JsonSerializer.ToBytes(stat);
                var sendTasks = _clients.Select(x => x.Value.SendAsync(bytes));
                await Task.WhenAll(sendTasks);
                await Task.Delay(TimeSpan.FromSeconds(30));
            }
        }
    }
}