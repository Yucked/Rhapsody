using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Frostbyte.Entities;
using Frostbyte.Entities.Enums;
using Frostbyte.Entities.EventArgs;
using Frostbyte.Entities.Infos;
using Frostbyte.Factories;

namespace Frostbyte.Server
{
    public sealed class WebsocketServer : IAsyncDisposable
    {
        private readonly CancellationTokenSource _cancellation;
        private readonly ConcurrentDictionary<IPEndPoint, WebsocketClient> _clients;
        private readonly Configuration _config;
        private readonly HttpListener _listener;
        private readonly SourceFactory _sourceFactory;

        public WebsocketServer()
        {
            _config = Singleton.Of<Configuration>();
            _sourceFactory = Singleton.Of<SourceFactory>();

            var prefix = $"http://{_config.Server.Hostname}:{_config.Server.Port}/";
            _listener = new HttpListener();
            _listener.Prefixes.Add(prefix);
            _listener.Start();

            _cancellation = new CancellationTokenSource();
            _clients = new ConcurrentDictionary<IPEndPoint, WebsocketClient>();
            LogFactory.Information<WebsocketServer>($"Websocket server started on: {prefix}");
        }

        public async ValueTask DisposeAsync()
        {
            foreach (var (_, client) in _clients)
                await client.DisposeAsync()
                    .ConfigureAwait(false);

            _cancellation.Cancel(false);
            _clients.Clear();
            _listener.Close();
        }

        public async Task InitializeAsync()
        {
            LogFactory.Information<WebsocketServer>("Server now listening for connections.");

            _ = CleanupClientsAsync()
                .ConfigureAwait(false);

            _ = SendMetricsAsync()
                .ConfigureAwait(false);

            while (!_cancellation.IsCancellationRequested)
            {
                var context = await _listener.GetContextAsync()
                    .ConfigureAwait(false);

                var authorization = context.Request.Headers.Get("Authorization");
                if (authorization != _config.Server.Authorization)
                {
                    CloseResponse(context, 401, $"Failed to authorize request from {context.Request.RemoteEndPoint}.");
                    return;
                }

                try
                {
                    if (context.Request.IsWebSocketRequest)
                    {
                        LogFactory.Debug<WebsocketServer>(
                            $"Processing incoming websocket request from {context.Request.RemoteEndPoint}.");

                        await ProcessWebSocketAsync(context)
                            .ConfigureAwait(false);
                    }
                    else
                    {
                        LogFactory.Debug<WebsocketServer>(
                            $"Processing incoming rest request from {context.Request.RemoteEndPoint}.");

                        await ProcessRestRequest(context)
                            .ConfigureAwait(false);
                    }
                }
                catch (Exception ex)
                {
                    CloseResponse(context, 500, "Something went wrong when trying to process request.");
                    LogFactory.Error<WebsocketServer>(exception: ex);
                }
            }
        }

        private async Task ProcessWebSocketAsync(HttpListenerContext context)
        {
            var endpoint = context.Request.RemoteEndPoint;
            if (context.Request.Url.LocalPath != "/")
            {
                CloseResponse(context, 404, $"{endpoint} requested an unknown path: {context.Request.Url.LocalPath}");
                return;
            }

            if (_clients.TryGetValue(endpoint, out var client))
            {
                await client.CloseAsync("Connection already exists. Please try again connecting.")
                    .ConfigureAwait(false);

                await client.DisposeAsync()
                    .ConfigureAwait(false);

                _clients.TryRemove(endpoint, out _);
                LogFactory.Warning<WebsocketServer>($"Connection from {endpoint} denied. Connection already exists.");
                return;
            }

            var userId = context.Request.Headers.Get("User-Id");
            if (string.IsNullOrWhiteSpace(userId))
            {
                CloseResponse(context, 401, $"{endpoint} client didn't send User-Id error. Websocket request denied.");
                return;
            }

            var wsContext = await context.AcceptWebSocketAsync(null)
                .ConfigureAwait(false);
            client = new WebsocketClient(wsContext, ulong.Parse(userId), _config.Server.BufferSize);
            _clients.TryAdd(endpoint, client);
            LogFactory.Information<WebsocketServer>(
                $"Websocket connection established from {context.Request.RemoteEndPoint}.");

            _ = client.ReceiveAsync()
                .ConfigureAwait(false);
            LogFactory.Debug<WebsocketServer>($"Started receiving data from {context.Request.RemoteEndPoint}.");
        }

        private async Task ProcessRestRequest(HttpListenerContext context)
        {
            var endpoint = context.Request.RemoteEndPoint;
            if (context.Request.Url.LocalPath != "/tracks")
            {
                CloseResponse(context, 404, $"{endpoint} requested an unknown path: {context.Request.Url.LocalPath}");
                return;
            }

            var (provider, query) = context.Request.QueryString.BuildQuery();
            if (string.IsNullOrWhiteSpace(provider) || string.IsNullOrWhiteSpace(query))
            {
                CloseResponse(context, 400, "Missing provider and/or query argument.");
                return;
            }

            var search = await _sourceFactory.ProcessRequestAsync(provider, query)
                .ConfigureAwait(false);

            await context.Response.OutputStream.WriteAsync(search.Serialize())
                .ConfigureAwait(false);

            context.Response.StatusCode = 200;
            context.Response.Close();

            LogFactory.Debug<WebsocketServer>($"Replied to {endpoint} with {context.Response.StatusCode}.");
        }

        private async Task CleanupClientsAsync()
        {
            LogFactory.Information<WebsocketServer>("Started up websocket client cleanup task.");
            while (!_cancellation.IsCancellationRequested)
            {
                foreach (var (endPoint, client) in _clients)
                    if (client.IsConnected)
                    {
                        foreach (var (guildId, voice) in client.Voices)
                        {
                            if (voice.IsConnected)
                                continue;

                            client.Voices.TryRemove(guildId, out _);
                            LogFactory.Debug<WebsocketServer>(
                                $"Removed {guildId} voice connection since client is disposed.");
                        }
                    }
                    else
                    {
                        _clients.TryRemove(endPoint, out _);
                        LogFactory.Warning<WebsocketServer>(
                            $"Removed {endPoint} from clients since client is disposed.");
                    }

                await Task.Delay(TimeSpan.FromSeconds(15), _cancellation.Token)
                    .ConfigureAwait(false);
            }
        }

        private async Task SendMetricsAsync()
        {
            LogFactory.Information<WebsocketServer>("Started up metrics sender task.");
            var process = Process.GetCurrentProcess();
            while (!_cancellation.IsCancellationRequested)
            {
                if (!_clients.IsEmpty)
                {
                    var metricsInfo = new MetricsInfo
                    {
                        ConnectedClients = _clients.Count,
                        ConnectedPlayers = _clients.Values.Sum(x => x.Voices.Count),
                        PlayingPlayers =
                            _clients.Values.Sum(x => x.Voices.Values.Count(p => p.Player.State == PlayerState.Playing)),
                        Uptime = (long) (DateTime.Now - process.StartTime).TotalMilliseconds,
                        Memory = new MemoryInfo(process.VirtualMemorySize64, process.WorkingSet64),
                        Cpu = new CpuInfo(Environment.ProcessorCount, 0, 0)
                    };

                    var metricsEvent = new MetricsEvent(metricsInfo);
                    var sendTasks = _clients.Values.Select(x => x.SendAsync(metricsEvent));
                    await Task.WhenAll(sendTasks)
                        .ConfigureAwait(false);

                    LogFactory.Debug<WebsocketServer>(JsonSerializer.Serialize(metricsEvent));
                }

                await Task.Delay(TimeSpan.FromMinutes(3))
                    .ConfigureAwait(false);
            }
        }

        private void CloseResponse(HttpListenerContext context, int code, string reason)
        {
            var response = context.Response;
            response.StatusCode = code;
            response.StatusDescription = reason;
            response.Close();

            LogFactory.Warning<WebsocketServer>(reason);
        }
    }
}