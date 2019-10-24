using System;
using System.Net.WebSockets;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Concept.Caches;
using Concept.Entities;
using Microsoft.Extensions.Logging;

namespace Concept.Controllers
{
    public class SocketControllerBase
    {
        public ClientsCache Cache { get; }
        private readonly ILogger<SocketControllerBase> _logger;

        protected SocketControllerBase(ClientsCache clientsCache, ILogger<SocketControllerBase> logger)
        {
            Cache = clientsCache;
            _logger = logger;
        }

        public virtual Task OnConnectedAsync(SocketConnection connection)
        {
            Cache.AddConnection(connection);
            _logger.LogInformation($"User with {connection.UserId} snowflake connected!");
            return Task.CompletedTask;
        }

        public virtual async Task OnDisconnectedAsync(SocketConnection connection)
        {
            _logger.LogError($"User with {connection.UserId} snowflake disconnected.");

            try
            {
                await connection.Socket.CloseAsync(WebSocketCloseStatus.NormalClosure,
                    "Closed by remote.", CancellationToken.None);
                Cache.RemoveConnection(connection.UserId);
            }
            catch (Exception exception)
            {
                _logger.LogCritical(exception, exception.Message);
            }
        }

        protected async Task SendMessageAsync(WebSocket socket, object data)
        {
            if (socket.State != WebSocketState.Open)
                return;

            var raw = JsonSerializer.SerializeToUtf8Bytes(data);
            await socket.SendAsync(raw, WebSocketMessageType.Text, true, CancellationToken.None);
        }

        public async Task SendToAllAsync(object data)
        {
            foreach (var value in Cache.Clients.Values)
            {
                if (value.Socket.State != WebSocketState.Open)
                    continue;

                await SendMessageAsync(value.Socket, data);
            }
        }

        public virtual Task ReceiveAsync(SocketConnection connection, ReadOnlyMemory<byte> buffer)
            => Task.CompletedTask;
    }
}