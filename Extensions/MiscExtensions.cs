using Frostbyte.Entities;
using Frostbyte.Entities.Audio;
using Frostbyte.Handlers;
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

        public static bool VerifyTask(this Task task)
        {
            return task.IsCanceled || task.IsFaulted || task.Exception != null;
        }

        public static async Task ReceiveAsync<TClass, TJson>(this WebSocket socket, CancellationTokenSource tokenSource, Func<TJson, Task> func)
        {
            try
            {
                while (!tokenSource.IsCancellationRequested && socket.State == WebSocketState.Open)
                {
                    var memory = new Memory<byte>();
                    var result = await socket.ReceiveAsync(memory, tokenSource.Token)
                        .ConfigureAwait(false);

                    switch (result.MessageType)
                    {
                        case WebSocketMessageType.Close:
                            await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None)
                                         .ConfigureAwait(false);
                            break;

                        case WebSocketMessageType.Text:
                            var parse = JsonSerializer.Parse<TJson>(memory.Span);
                            await func.Invoke(parse)
                                .ConfigureAwait(false);
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                LogHandler<TClass>.Log.Error(ex?.InnerException ?? ex);
            }
            finally
            {
                socket?.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None)
                    .ConfigureAwait(false);
            }
        }
    }
}