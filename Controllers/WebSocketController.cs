using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using Concept.WebSockets;
using Microsoft.Extensions.Logging;

namespace Concept.Controllers
{
    public sealed class WebSocketController : SocketControllerBase
    {
        private readonly ILogger<WebSocketController> _logger;

        public WebSocketController(ClientsCache clientsClients, ILogger<WebSocketController> logger)
            : base(clientsClients)
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