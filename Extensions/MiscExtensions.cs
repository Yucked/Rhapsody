using Frostbyte.Entities;
using Frostbyte.Entities.Audio;
using System;
using System.Collections.Specialized;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
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

        public static (string Provider, string Query) BuildQuery(this NameValueCollection collection)
        {
            var provider = collection.Get("prov");
            collection.Remove("prov");
            var query = string.Empty;

            for (var i = 0; i < collection.Count; i++)
            {
                var key = collection.GetKey(i);
                var keyValue = collection.Get(i);

                if (key == "q")
                {
                    query += keyValue;
                }
                else
                {
                    query += $"&{key}={keyValue}";
                }
            }

            return (provider, query);
        }

        public static bool IsSourceEnabled(this AudioSources sources, string source)
        {
            var prop = sources.GetType().GetProperty(source);
            return prop.GetValue(sources).TryCast<bool>();
        }

        public static string EncodeTrack(this AudioTrack track, string provider)
        {
            var str = $"{provider}:{track.Id}:{track.Title}:{track.Url}";
            var bytes = Encoding.UTF8.GetBytes(str);
            return Convert.ToBase64String(bytes, Base64FormattingOptions.None);
        }
    }
}