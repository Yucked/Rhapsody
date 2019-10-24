using System;
using System.Text.Json;
using System.Threading.Tasks;
using Concept.Caches;
using Concept.Entities;
using Concept.Entities.Payloads;
using Concept.Entities.Payloads.Inbound;
using Microsoft.Extensions.Logging;
using Vysn.Commons;
using Vysn.Voice;
using Vysn.Voice.Enums;
using Vysn.Voice.Packets;

namespace Concept.Controllers
{
    public sealed class WebSocketController : SocketControllerBase
    {
        private readonly ILogger _logger;

        public WebSocketController(ClientsCache cacheCache, ILogger<WebSocketController> logger)
            : base(cacheCache, logger)
        {
            _logger = logger;
        }

        /// <inheritdoc />
        public override async Task ReceiveAsync(SocketConnection connection, ReadOnlyMemory<byte> buffer)
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
                        UserId = Snowflake.FromId(connection.UserId),
                        Token = connectPayload.Token,
                        SessionId = connectPayload.SessionId,
                        Endpoint = connectPayload.Endpoint
                    });
                    connection.AddGatewayClient(gatewayClient);

                    await SendMessageAsync(connection.Socket, "Connected accepted!");
                    break;
            }

            await base.ReceiveAsync(connection, buffer);
        }

        private Task OnLogAsync(LogMessage logMessage)
        {
            _logger.Log(logMessage.Level.ConvertTo(), logMessage.Exception, logMessage.Message);
            return Task.CompletedTask;
        }
    }
}