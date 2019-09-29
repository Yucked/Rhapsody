using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace Concept.WebSockets
{
    public class ConceptWebSocket : WebSocketHandler
    {
        protected readonly ILogger<ConceptWebSocket> _logger;

        public ConceptWebSocket(WebSocketConnectionManager webSocketConnectionManager, ILogger<ConceptWebSocket> logger)
            : base(webSocketConnectionManager)
        {
            _logger = logger;
        }

        public override async Task ReceiveAsync(WebSocket socket, WebSocketReceiveResult result, byte[] buffer)
        {
            _logger.Log(LogLevel.Information, $"Message received: {Encoding.UTF8.GetString(buffer)}");
            await SendMessageAsync(socket, "Hello, I'm Concept");
            await base.ReceiveAsync(socket, result, buffer);
        }
    }
}