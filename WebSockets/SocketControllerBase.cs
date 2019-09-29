using System.Net;
using System.Net.WebSockets;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Concept.WebSockets
{
    public class SocketControllerBase
    {
        public ClientsCache Clients { get; }

        protected SocketControllerBase(ClientsCache clientsClients)
        {
            Clients = clientsClients;
        }

        public virtual Task OnConnectedAsync(IPAddress address, WebSocket socket)
        {
            Clients.AddClient(address, socket);
            return Task.CompletedTask;
        }

        public virtual async Task OnDisconnectedAsync(IPAddress address, WebSocket socket)
        {
            await socket.CloseAsync(WebSocketCloseStatus.NormalClosure,
                "Closed by remote.", CancellationToken.None);

            Clients.RemoveClient(address);
        }

        protected async Task SendMessageAsync(WebSocket socket, object data)
        {
            if (socket.State != WebSocketState.Open)
                return;

            var raw = JsonSerializer.SerializeToUtf8Bytes(data);
            await socket.SendAsync(raw, WebSocketMessageType.Text, true, CancellationToken.None);
        }

        public async Task SendMessageAsync(IPAddress address, string message)
            => await SendMessageAsync(Clients.GetClient(address), message);

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