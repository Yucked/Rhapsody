using System;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Concept.Caches;
using Concept.Payloads;
using Concept.Payloads.InboundPayloads;
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
            var message = Encoding.UTF8.GetString(buffer.Span);

            var result = this.GetPayloadFromBuffer(buffer);

            //await SendMessageAsync(socket, $"Pong {message}");
            await base.ReceiveAsync(socket, buffer);
        }

        public IPayload GetPayloadFromBuffer(ReadOnlyMemory<byte> buffer)
        {
            var reader = new Utf8JsonReader(buffer.Span);

            IPayload payload = new InboundPayload();

            if (reader.TokenType != JsonTokenType.StartObject)
                throw new JsonException("Websocket did not receive a JSON payload.");

            for(int i = 0; reader.TokenType != JsonTokenType.EndObject; i++)
            {
                PayloadOp op = PayloadOp.Connect;

                if(i == 2)
                {
                    //Check if op was either sent as a number or a string
                    if(reader.TokenType == JsonTokenType.Number)
                        switch(reader.GetInt32())
                        {
                            case 1:
                                payload = new ConnectPayload();
                                BuildConnectPayload(ref payload, ref reader);
                                break;

                            case 2:
                                //Repeat the above case for every case
                                op = PayloadOp.Play;
                                payload = new PlayPayload();
                                BuildPlayPayload(ref payload, ref reader);
                                break;

                            case 3:
                                op = PayloadOp.Stop;
                                BuildInboundPayload(ref payload, ref reader);
                                break;

                            case 4:
                                op = PayloadOp.Pause;
                                payload = new PausePayload();
                                BuildPausePayload(ref payload, ref reader);
                                break;

                            case 5:
                                op = PayloadOp.Seek;
                                payload = new SeekPayload();
                                BuildSeekPayload(ref payload, ref reader);
                                break;

                            case 6:
                                op = PayloadOp.Volume;
                                payload = new VolumePayload();
                                BuildVolumePayload(ref payload, ref reader);
                                break;

                            case 7:
                                op = PayloadOp.Destroy;
                                BuildInboundPayload(ref payload, ref reader);
                                break;
                                //Continue this for all payloads
                        }
                    else if(reader.TokenType == JsonTokenType.String)
                        switch(reader.GetString())
                        {
                            case "connect":
                                payload = new ConnectPayload();
                                BuildConnectPayload(ref payload, ref reader);
                                break;

                            case "play":
                                //Repeat the above case for every case
                                op = PayloadOp.Play;
                                payload = new PlayPayload();
                                BuildPlayPayload(ref payload, ref reader);
                                break;

                            case "stop":
                                op = PayloadOp.Stop;
                                BuildInboundPayload(ref payload, ref reader);
                                break;

                            case "pause":
                                op = PayloadOp.Pause;
                                payload = new PausePayload();
                                BuildPausePayload(ref payload, ref reader);
                                break;

                            case "seek":
                                op = PayloadOp.Seek;
                                payload = new SeekPayload();
                                BuildSeekPayload(ref payload, ref reader);
                                break;

                            case "volume":
                                op = PayloadOp.Volume;
                                payload = new VolumePayload();
                                BuildVolumePayload(ref payload, ref reader);
                                break;

                            case "destroy":
                                op = PayloadOp.Destroy;
                                BuildInboundPayload(ref payload, ref reader);
                                break;
                                //Continue this for all payloads
                        }

                    payload.Op = op;
                }
            }
            return payload;
        }

        public void BuildInboundPayload(ref IPayload payload, ref Utf8JsonReader reader)
        {

        }

        public void BuildConnectPayload(ref IPayload payload, ref Utf8JsonReader reader)
        {
            //Use builder for further byte parsing here.
        }

        public void BuildPlayPayload(ref IPayload payload, ref Utf8JsonReader reader)
        {

        }

        public void BuildPausePayload(ref IPayload payload, ref Utf8JsonReader reader)
        {

        }

        public void BuildVolumePayload(ref IPayload payload, ref Utf8JsonReader reader)
        {

        }

        public void BuildSeekPayload(ref IPayload payload, ref Utf8JsonReader reader)
        {

        }
    }
}