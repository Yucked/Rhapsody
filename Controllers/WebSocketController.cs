using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using Concept.Caches;
using Microsoft.Extensions.Logging;

namespace Concept.Controllers
{
    public sealed class WebSocketController : SocketControllerBase
    {
        private readonly ILogger<WebSocketController> _logger;

        public WebSocketController(ClientsCache clientsClients, ILogger<WebSocketController> logger)
            : base(clientsClients, logger)
        {
            _logger = logger;
        }

        public override async Task ReceiveAsync(WebSocket socket, ReadOnlyMemory<byte> buffer)
        {
            _logger.Log(LogLevel.Debug, $"Message received: {Encoding.UTF8.GetString(buffer.Span)}");
            var message = Encoding.UTF8.GetString(buffer.Span.Slice(5));
            await SendMessageAsync(socket, $"Pong {message}");
            await base.ReceiveAsync(socket, buffer);
        }
    }
}