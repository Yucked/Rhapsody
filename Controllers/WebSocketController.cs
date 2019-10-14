using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using Concept.Caches;
using Concept.Payloads.InboundPayloads;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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
            var message = Encoding.UTF8.GetString(buffer.Span);
            _logger.LogDebug($"Message received: {message}");
            var json = JObject.Parse(message);
            var op = json.Value<string>("op");
            object payload;
            switch(op)
            {
                case "connect":

                    _logger.LogInformation(json.ToString()); //Probably replace this with a more informative response compared to ll
                    payload = JsonConvert.DeserializeObject<ConnectPayload>(json.ToString());

                    //Method to send payload to

                    break;

                case "play":

                    _logger.LogInformation(json.ToString());
                    payload = JsonConvert.DeserializeObject<PlayPayload>(json.ToString());

                    //Method to send payload to

                    break;

                case "stop":

                    _logger.LogInformation(json.ToString());
                    payload = JsonConvert.DeserializeObject<InboundPayload>(json.ToString());

                    //Method to send payload to
                    break;

                case "pause":

                    _logger.LogInformation(json.ToString());
                    payload = JsonConvert.DeserializeObject<PausePayload>(json.ToString());
                    //Method to send payload to

                    break;

                case "seek":

                    _logger.LogInformation(json.ToString());
                    payload = JsonConvert.DeserializeObject<SeekPayload>(json.ToString());
                    //Method to send payload to

                    break;

                case "volume":

                    _logger.LogInformation(json.ToString());
                    payload = JsonConvert.DeserializeObject<VolumePayload>(json.ToString());
                    //Method to send payload to

                    break;

                case "destroy":

                    _logger.LogInformation(json.ToString());
                    payload = JsonConvert.DeserializeObject<InboundPayload>(json.ToString());
                    //Method to send payload to.
                    break;

                default:

                    _logger.LogError($"Invalid operation: {op}");
                    return;
            }

            //await SendMessageAsync(socket, $"Pong {message}");
            await base.ReceiveAsync(socket, buffer);
        }
    }
}