using System;
using System.Net.WebSockets;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Concept.Caches;
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

        public virtual Task OnConnectedAsync(ulong snowflake, WebSocket socket)
        {
            Clients.AddClient(snowflake, socket);
            _logger.LogInformation($"User with {snowflake} snowflake connected!");
            return Task.CompletedTask;
        }

        public virtual async Task OnDisconnectedAsync(ulong snowflake, WebSocket socket)
        {
            _logger.LogError($"User with {snowflake} snowflake disconnected.");
            Clients.RemoveClient(snowflake);

            try
            {
                await socket.CloseAsync(WebSocketCloseStatus.NormalClosure,
                    "Closed by remote.", CancellationToken.None);
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
                if (value.State != WebSocketState.Open)
                    continue;

                await SendMessageAsync(value, message);
            }
        }

        public virtual Task ReceiveAsync(WebSocket socket, ReadOnlyMemory<byte> buffer)
            => Task.CompletedTask;
    }
}