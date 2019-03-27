using Fleck;
using Frostbyte.Attributes;
using Frostbyte.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Frostbyte.Handlers
{
    [Service(ServiceLifetime.Singleton)]
    public sealed class WebsocketHandler
    {
        private ILogger Logger { get; }

        public WebsocketHandler(ILoggerFactory factory)
        {
            Logger = factory.CreateLogger<WebsocketHandler>();
        }

        public void Initialize(ConfigEntity config)
        {
            Logger.LogInformation($"Starting up websocket server at: ws://{config.Host}:{config.Port}");
            FleckLog.LogAction = (lvl, msg, ex) => { };

            var server = new WebSocketServer($"ws://{config.Host}:{config.Port}");
            server.Start(x =>
            {
                x.OnOpen = () =>
                {
                    Logger.LogInformation("Websocket server started!");
                };

                x.OnClose = () =>
                {

                };

                x.OnError = ex =>
                {
                    Logger.LogCritical(ex, default);
                };

                x.OnMessage = msg =>
                {
                    Logger.LogDebug(msg);
                };
            });
        }
    }
}