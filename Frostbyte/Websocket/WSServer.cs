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

namespace Frostbyte.Websocket
{
    public sealed class WsServer : IAsyncDisposable
    {
        private readonly ConcurrentDictionary<IPEndPoint, WsClient> _clients;
        private readonly HttpListener _listener;
        private readonly CancellationTokenSource _mainCancellation, _wsCancellation, _statsCancellation;
        private readonly SourceHandler _sourceHandler;
        private readonly IServiceProvider _provider;

        private Configuration _config;
        private CancellationTokenSource _receiveCancellation;
        private Task _statsSenderTask;

        public WsServer(SourceHandler sourceHandler, Configuration configuration, IServiceProvider provider)
        {
            _listener = new HttpListener();
            _clients = new ConcurrentDictionary<IPEndPoint, WsClient>();
            _wsCancellation = new CancellationTokenSource();
            _statsCancellation = new CancellationTokenSource();
            _mainCancellation = CancellationTokenSource.CreateLinkedTokenSource(_wsCancellation.Token, _statsCancellation.Token);
            _sourceHandler = sourceHandler;
            _provider = provider;
            _config = configuration;
            Singletons.SetConfig(configuration);
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
            LogHandler<WsServer>.Log.Information("TLS11, TLS12 & TLS13 set as security protocol.");
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13;

            _listener.Prefixes.Add(_config.Url);
            _listener.Start();
            LogHandler<WsServer>.Log.Information($"Server started on {_config.Url}.");

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
                        LogHandler<WsServer>.Log.Debug($"Incoming http request from {remoteEndPoint}.");
                        if (context.Request.Headers.Get("Password") != _config.Password)
                        {
                            response.Reason = "Password header doesn't match value specified in configuration";
                            await context.SendResponseAsync(response).ConfigureAwait(false);
                        }
                        else
                        {
                            var (prov, query) = context.Request.QueryString.BuildQuery();

                            if (query is null || prov is null)
                            {
                                response.Reason = "Please use the `?prov={provider}&q={YOUR_QUERY} argument after /tracks";
                                await context.SendResponseAsync(response).ConfigureAwait(false);
                            }
                            else
                            {
                                response = await _sourceHandler.HandleRequestAsync(prov, query, _provider).ConfigureAwait(false);
                                await context.SendResponseAsync(response).ConfigureAwait(false);
                            }
                        }

                        LogHandler<WsServer>.Log.Debug($"Replied to {remoteEndPoint} with {response.Reason}.");
                        break;

                    case "/":
                        if (!context.Request.IsWebSocketRequest)
                        {
                            response.Reason = "Only websocket connections are allowed at this endpoint. For rest use /tracks endpoint.";
                            await context.SendResponseAsync(response).ConfigureAwait(false);
                            return;
                        }

                        LogHandler<WsServer>.Log.Debug($"Incoming websocket request coming from {remoteEndPoint}.");

                        var wsContext = await context.AcceptWebSocketAsync(default).ConfigureAwait(false);
                        var wsClient = new WsClient(wsContext, remoteEndPoint);
                        wsClient.OnClosed += OnClosed;
                        _clients.TryAdd(remoteEndPoint, wsClient);
                        _ = wsClient.ReceiveAsync(_receiveCancellation);

                        LogHandler<WsServer>.Log.Information($"Websocket connection opened from {remoteEndPoint}.");
                        break;

                    default:
                        LogHandler<WsServer>.Log.Warning($"{remoteEndPoint} requested an unknown path: {context.Request.Url}.");
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
                LogHandler<WsServer>.Log.Error(ex);
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
                    ConnectedPlayers = _clients.Values.Sum(x => x.Handlers.Count),
                    PlayingPlayers = _clients.Values.Sum(x => x.Handlers.Values.Count(g => g.PlaybackEngine.IsPlaying is true)),
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