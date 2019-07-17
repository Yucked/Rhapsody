using System;
using System.Collections.Concurrent;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Frostbyte.Entities;
using Frostbyte.Factories;
using Frostbyte.Misc;

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
                        LogFactory.Debug<WebsocketServer>($"Processing incoming websocket request from {context.Request.RemoteEndPoint}.");

                        await ProcessWebSocketAsync(context)
                            .ConfigureAwait(false);
                    }
                    else
                    {
                        LogFactory.Debug<WebsocketServer>($"Processing incoming rest request from {context.Request.RemoteEndPoint}.");

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
            LogFactory.Information<WebsocketServer>($"Websocket connection established from {context.Request.RemoteEndPoint}.");

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

            LogFactory.Information<WebsocketServer>($"Replied to {endpoint} with {context.Response.StatusCode}.");
        }

        private async Task CleanupClientsAsync()
        {
            while (!_clients.IsEmpty)
            {
                foreach (var (endPoint, client) in _clients)
                {
                    if (!client.IsDisposed)
                        continue;

                    _clients.TryRemove(endPoint, out _);
                }

                await Task.Delay(TimeSpan.FromSeconds(15), _cancellation.Token)
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