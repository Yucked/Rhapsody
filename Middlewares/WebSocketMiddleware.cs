using Microsoft.AspNetCore.Http;
using System;
using System.Buffers;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Concept.Controllers;
using Microsoft.Extensions.Logging;

namespace Concept.Middlewares
{
    public sealed class WebSocketMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly string _authorization;
        private readonly ILogger<WebSocketMiddleware> _logger;

        public WebSocketMiddleware(RequestDelegate next, string authorization, ILogger<WebSocketMiddleware> logger)
        {
            _next = next;
            _authorization = authorization;
            _logger = logger;
        }

        //ASP.Net Core will pass the dependecies to us.
        public async Task Invoke(HttpContext context, WebSocketController controller)
        {
            if (context.WebSockets.IsWebSocketRequest)
            {
                if (!IsValidRequest(context, out var snowflake))
                    return;

                _logger.LogInformation($"Incoming websocket request from {context.Connection.RemoteIpAddress}.");

                var socket = await context.WebSockets.AcceptWebSocketAsync();
                await controller.OnConnectedAsync(snowflake, socket);

                await Receive(socket, async (result, buffer) =>
                {
                    switch (result.MessageType)
                    {
                        case WebSocketMessageType.Text:
                            await controller.ReceiveAsync(socket, result, buffer);
                            return;

                        case WebSocketMessageType.Close:
                            await controller.OnDisconnectedAsync(snowflake, socket);
                            _logger.LogWarning(
                                $"Client ({context.Connection.RemoteIpAddress}) disconnected.");
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

        private bool IsValidRequest(HttpContext context, out ulong snowflake)
        {
            if (!context.Request.Headers.TryGetValue("Authorization", out var auth)
                && !_authorization.Equals(auth))
            {
                context.Response.StatusCode = 401;
                snowflake = default;
                return false;
            }

            if (!context.Request.Headers.TryGetValue("User-Id", out var userId))
            {
                context.Response.StatusCode = 403;
                snowflake = default;
                return false;
            }


            if (!ulong.TryParse(userId, out snowflake))
            {
                context.Response.StatusCode = 403;
                return false;
            }

            context.Response.StatusCode = 101;
            return true;
        }
    }
}