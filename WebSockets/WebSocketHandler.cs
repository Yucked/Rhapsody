using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Concept.WebSockets
{
    public class WebSocketHandler
    {
        protected WebSocketConnectionManager WebSocketConnectionManager { get; set; }

        public WebSocketHandler(WebSocketConnectionManager webSocketConnectionManager)
        {
            WebSocketConnectionManager = webSocketConnectionManager;
        }

        public virtual Task OnConnected(WebSocket socket)
        {
            WebSocketConnectionManager.AddSocket(socket);
            return Task.CompletedTask;
        }

        public virtual async Task OnDisconnected(WebSocket socket)
            => await WebSocketConnectionManager.RemoveSocket(WebSocketConnectionManager.GetId(socket));

        public async Task SendMessageAsync(WebSocket socket, string message)
        {
            if (socket.State != WebSocketState.Open)
                return;

            await socket.SendAsync(buffer: new ArraySegment<byte>(array: Encoding.ASCII.GetBytes(message),
                                                                  offset: 0,
                                                                  count: message.Length),
                                   messageType: WebSocketMessageType.Text,
                                   endOfMessage: true,
                                   cancellationToken: CancellationToken.None);
        }

        public async Task SendMessageAsync(string socketId, string message)
            => await SendMessageAsync(WebSocketConnectionManager.GetSocketById(socketId), message);

        public async Task SendMessageToAllAsync(string message)
        {
            foreach (var socket in WebSocketConnectionManager.GetAll())
            {
                if (socket.Value.State == WebSocketState.Open)
                    await SendMessageAsync(socket.Value, message);
            }
        }

        public virtual Task ReceiveAsync(WebSocket socket, WebSocketReceiveResult result, byte[] buffer)
            => Task.CompletedTask;
    }
}