using System.Net.WebSockets;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Concept.WebSockets
{
    public class SocketControllerBase
    {
        public ClientsCache Clients { get; }
        public ILogger<SocketControllerBase> Logger { get; }

        protected SocketControllerBase(ClientsCache clientsClients, ILogger<SocketControllerBase> logger)
        {
            Clients = clientsClients;
            Logger = logger;
        }

        public virtual Task OnConnectedAsync(ulong snowflake, WebSocket socket)
        {
            Clients.AddClient(snowflake, socket);
            Logger.LogInformation($"User with {snowflake} snowflake connected!");
            return Task.CompletedTask;
        }

        public virtual async Task OnDisconnectedAsync(ulong snowflake, WebSocket socket)
        {
            await socket.CloseAsync(WebSocketCloseStatus.NormalClosure,
                "Closed by remote.", CancellationToken.None);

            Logger.LogError($"User with {snowflake} snowflake disconnected.");
            Clients.RemoveClient(snowflake);
        }

        protected async Task SendMessageAsync(WebSocket socket, object data)
        {
            if (socket.State != WebSocketState.Open)
                return;

            var raw = JsonSerializer.SerializeToUtf8Bytes(data);
            await socket.SendAsync(raw, WebSocketMessageType.Text, true, CancellationToken.None);
        }

        public async Task SendMessageAsync(ulong snowflake, string message)
            => await SendMessageAsync(Clients.GetClient(snowflake), message);

        public async Task SendMessageToAllAsync(string message)
        {
            foreach (var (_, value) in Clients.GetAll())
            {
                if (value.State != WebSocketState.Open)
                    continue;

                await SendMessageAsync(value, message);
            }
        }

        public virtual Task ReceiveAsync(WebSocket socket, WebSocketReceiveResult result, byte[] buffer)
            => Task.CompletedTask;
    }
}