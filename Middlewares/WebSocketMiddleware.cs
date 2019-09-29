using Concept.WebSockets;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace Concept.Middlewares
{
    public class WebSocketMiddleware
    {
        //Thats represent the request, if we do await next(); we pass the request to the next Middleware.
        private readonly RequestDelegate next;

        public WebSocketMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        //ASP.Net Core will pass the dependecies to us.
        public async Task Invoke(HttpContext context, ConceptWebSocket webSocketHandler)
        {
            if (context.WebSockets.IsWebSocketRequest)
            {
                var socket = await context.WebSockets.AcceptWebSocketAsync();

                await webSocketHandler.OnConnected(socket);

                await Receive(socket, async (result, buffer) =>
                {
                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        await webSocketHandler.ReceiveAsync(socket, result, buffer);
                        return;
                    }
                    else if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await webSocketHandler.OnDisconnected(socket);
                        return;
                    }
                });
            }
            else
            {
                await next(context);
            }
        }

        private async Task Receive(WebSocket socket, Action<WebSocketReceiveResult, byte[]> handleMessage)
        {
            var buffer = new byte[1024 * 4];

            while (socket.State == WebSocketState.Open)
            {
                var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                handleMessage(result, buffer);
            }
        }
    }
}