using System.Net;
using System.Net.WebSockets;
using System.Text.Json.Serialization;
using System.Text.Utf8;
using System.Threading;
using System.Threading.Tasks;
using Frostbyte.Handlers;

namespace Frostbyte.Extensions
{
    public static class MiscExtensions
    {
        public static ValueTask SendResponseAsync(this HttpListenerContext context, object @object)
        {
            return context.Response.OutputStream.WriteAsync(JsonSerializer.ToUtf8Bytes(@object));
        }

        public static async ValueTask SendAsync<T>(this WebSocket socket, object data)
        {
            var bytes = JsonSerializer.ToUtf8Bytes(data);
            await socket.SendAsync(bytes, WebSocketMessageType.Text, true, CancellationToken.None);
            var str = new Utf8String(bytes);
            LogHandler<T>.Log.Debug(str.ToString());
        }

        public static T TryCast<T>(this object @object)
        {
            return @object is T value ? value : default;
        }
    }
}