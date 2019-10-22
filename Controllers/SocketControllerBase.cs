using System;
using System.Net.WebSockets;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Concept.Caches;
using Concept.Options;
using Microsoft.Extensions.Logging;

namespace Concept.Controllers
{
    public class SocketControllerBase
    {
        public ClientsCache Clients { get; }
        private readonly ILogger<SocketControllerBase> _logger;

        protected SocketControllerBase(ClientsCache clientsClients, ILogger<SocketControllerBase> logger)
        {
            Clients = clientsClients;
            _logger = logger;
        }

        public virtual Task OnConnectedAsync(ClientOptions options)
        {
            Clients.AddClient(options);
            _logger.LogInformation($"User with {options.UserId} snowflake connected!");
            return Task.CompletedTask;
        }

        public virtual async Task OnDisconnectedAsync(ClientOptions options)
        {
            _logger.LogError($"User with {options.UserId} snowflake disconnected.");

            try
            {
                await options.Socket.CloseAsync(WebSocketCloseStatus.NormalClosure,
                    "Closed by remote.", CancellationToken.None);
                Clients.RemoveClient(options.UserId);
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

        public async Task SendMessageToAllAsync(string message)
        {
            foreach (var (_, value) in Clients.GetAll())
            {
                if (value.Socket.State != WebSocketState.Open)
                    continue;

                await SendMessageAsync(value.Socket, message);
            }
        }

        public virtual Task ReceiveAsync(ClientOptions options, ReadOnlyMemory<byte> buffer)
            => Task.CompletedTask;
    }
}