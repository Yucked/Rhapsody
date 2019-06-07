using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Frostbyte.Extensions
{
    public static class MiscExtensions
    {
        public static ValueTask SendResponseAsync(this HttpListenerContext context, object @object)
        {
            return context.Response.OutputStream.WriteAsync(JsonSerializer.ToUtf8Bytes(@object));
        }

        public static async ValueTask SendAsync(this WebSocket socket, object data)
        {
            var bytes = JsonSerializer.ToUtf8Bytes(data);
            await socket.SendAsync(bytes, WebSocketMessageType.Text, true, CancellationToken.None);
            var str = Encoding.UTF8.GetString(bytes);
        }

        public static T TryCast<T>(this object @object)
        {
            return @object is T value ? value : default;
        }

        public static Regex Regex(this string pattern)
        {
            return new Regex(pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
        }
    }
}