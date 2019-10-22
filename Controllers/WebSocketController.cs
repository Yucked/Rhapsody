using System;
using System.Text.Json;
using System.Threading.Tasks;
using Concept.Caches;
using Concept.Options;
using Microsoft.Extensions.Logging;
using Test.Payloads;
using Test.Payloads.Inbound;
using Vysn.Commons;
using Vysn.Voice;
using Vysn.Voice.Enums;
using Vysn.Voice.Packets;

namespace Concept.Controllers
{
    public sealed class WebSocketController : SocketControllerBase
    {
        private readonly ILogger _logger;

        public WebSocketController(ClientsCache clientsClients, ILogger<WebSocketController> logger)
            : base(clientsClients, logger)
        {
            _logger = logger;
        }

        /// <inheritdoc />
        public override async Task ReceiveAsync(ClientOptions options, ReadOnlyMemory<byte> buffer)
        {
            var payload = JsonSerializer.Deserialize<BasePayload>(buffer.Span);

            switch (payload.PayloadType)
            {
                case PayloadType.Connect:
                    var connectPayload = JsonSerializer.Deserialize<ConnectPayload>(buffer.Span);
                    var gatewayClient = new VoiceGatewayClient(new VoiceConfiguration
                    {
                        Application = VoiceApplication.Music
                    });

                    gatewayClient.OnLog += OnLogAsync;
                    await gatewayClient.RunAsync(new ConnectionPacket
                    {
                        GuildId = Snowflake.FromId(payload.GuildId),
                        UserId = Snowflake.FromId(options.UserId),
                        Token = connectPayload.Token,
                        SessionId = connectPayload.SessionId,
                        Endpoint = connectPayload.Endpoint
                    });
                    options.AddGatewayClient(gatewayClient);
                    break;
            }

            await base.ReceiveAsync(options, buffer);
        }

        private Task OnLogAsync(LogMessage logMessage)
        {
            _logger.Log(logMessage.Level.ConvertTo(), logMessage.Exception, logMessage.Message);
            return Task.CompletedTask;
        }
    }
}