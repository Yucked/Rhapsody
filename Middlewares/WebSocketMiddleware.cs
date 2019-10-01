using Microsoft.AspNetCore.Http;
using System;
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

        public async Task Invoke(HttpContext context, WebSocketController controller)
        {
            if (!IsValidRequest(context, out var snowflake))
                return;

            if (!context.WebSockets.IsWebSocketRequest)
            {
                await _next(context);
                return;
            }

            _logger.LogInformation(
                $"Incoming websocket request from {context.Connection.RemoteIpAddress}:{context.Connection.RemotePort}.");

            var socket = await context.WebSockets.AcceptWebSocketAsync();
            await controller.OnConnectedAsync(snowflake, socket);
            await ReceiveAsync(snowflake, controller, context, socket);
        }

        private async Task ReceiveAsync(ulong snowflake, SocketControllerBase controller, HttpContext context,
            WebSocket socket)
        {
            try
            {
                while (socket.State == WebSocketState.Open)
                {
                    var buffer = new byte[512];
                    var result = await socket.ReceiveAsync(buffer, CancellationToken.None);
                    switch (result.MessageType)
                    {
                        case WebSocketMessageType.Text:
                            if (!result.EndOfMessage)
                                continue;

                            var lastIndex = Array.FindLastIndex(buffer, b => b != 0);
                            Array.Resize(ref buffer, lastIndex + 1);

                            await controller.ReceiveAsync(socket, buffer);
                            continue;

                        case WebSocketMessageType.Close:
                            _logger.LogWarning(
                                $"Client {context.Connection.RemoteIpAddress}:{context.Connection.RemotePort} disconnected.");
                            await controller.OnDisconnectedAsync(snowflake, socket);
                            continue;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, ex.Message);
                await controller.OnDisconnectedAsync(snowflake, socket);
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