using Microsoft.AspNetCore.Http;
using System;
using System.Buffers;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Concept.Controllers;

namespace Concept.Middlewares
{
    public sealed class WebSocketMiddleware
    {
        private readonly RequestDelegate _next;

        public WebSocketMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        //ASP.Net Core will pass the dependecies to us.
        public async Task Invoke(HttpContext context, WebSocketController controller)
        {
            if (context.WebSockets.IsWebSocketRequest)
            {
                var socket = await context.WebSockets.AcceptWebSocketAsync();
                await controller.OnConnectedAsync(context.Connection.RemoteIpAddress, socket);

                await Receive(socket, async (result, buffer) =>
                {
                    switch (result.MessageType)
                    {
                        case WebSocketMessageType.Text:
                            await controller.ReceiveAsync(socket, result, buffer);
                            return;

                        case WebSocketMessageType.Close:
                            await controller.OnDisconnectedAsync(context.Connection.RemoteIpAddress,
                                socket);
                            return;
                    }
                });
            }
            else
            {
                await _next(context);
            }
        }

        private async Task Receive(WebSocket socket, Action<WebSocketReceiveResult, byte[]> handleMessage)
        {
            var buffer = ArrayPool<byte>.Shared.Rent(512);

            while (socket.State == WebSocketState.Open)
            {
                var result = await socket.ReceiveAsync(buffer, CancellationToken.None);
                handleMessage(result, buffer);
            }
        }
    }
}