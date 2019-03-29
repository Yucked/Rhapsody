using Frostbyte.Attributes;
using Frostbyte.Entities;
using Frostbyte.Enums;
using Frostbyte.Handlers;
using Frostbyte.Websocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Frostbyte.Factories
{
    [Service(ServiceLifetime.Singleton)]
    public sealed class WebsocketFactory : IAsyncDisposable
    {
        private WSServer server;
        private readonly LogHandler<WebsocketFactory> _logger;
        private readonly ConcurrentDictionary<string, WSClient> _clients;

        public WebsocketFactory()
        {
            _logger = new LogHandler<WebsocketFactory>();
            _clients = new ConcurrentDictionary<string, WSClient>();
        }

        public void Initialize(ConfigEntity config)
        {
            _logger.LogInformation($"Starting ws server on: {config.Host}:{config.Port}.");
            server = new WSServer(config);

            _logger.Log($"{server.EndPoint.Address}" != config.Host
                ? LogLevel.Warning : LogLevel.Information,
                $"Server started on {server.EndPoint.Address}:{server.EndPoint.Port}.", null);

            server.OnConnected += OnConnected;
            server.OnDisconnected += OnDisconnected;
            server.OnMessageDelivered += OnMessageDelivered;
            server.OnMessageReceived += OnMessageReceived;

            server.Start();
        }

        private ValueTask OnConnected(WSClient client)
        {
            _logger.LogInformation($"");
            _clients.TryAdd(client.Id, client);
            return default;
        }

        private ValueTask OnDisconnected(WSClient client)
        {
            _clients.TryRemove(client.Id, out _);
            _logger.LogWarning($"Client with {client.Id} disconnected.");
            return default;
        }

        private ValueTask OnMessageDelivered(WSClient client, string message)
        {
            throw new NotImplementedException();
        }

        private ValueTask OnMessageReceived(WSClient client, string message)
        {
            throw new NotImplementedException();
        }

        public ValueTask DisposeAsync()
        {
            throw new NotImplementedException();
        }
    }
}