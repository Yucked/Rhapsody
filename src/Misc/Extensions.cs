using System;
using System.Collections.Specialized;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Frostbyte.Entities;
using Frostbyte.Entities.Enums;

namespace Frostbyte.Misc
{
    public static class Extensions
    {
        public static ReadOnlyMemory<byte> Serialize<T>(this T value)
        {
            var bytes = JsonSerializer.ToUtf8Bytes(value, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            return bytes;
        }

        public static T Deserialize<T>(this ReadOnlyMemory<byte> memory)
        {
            var parse = JsonSerializer.Parse<T>(memory.Span, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            return parse;
        }

        public static T Deserialize<T>(this byte[] bytes)
        {
            return Deserialize<T>(new ReadOnlyMemory<byte>(bytes));
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
                query += key == "q"
                    ? keyValue
                    : $"&{key}={keyValue}";
            }

            return (provider, query);
        }

        public static bool IsSourceEnabled(this SourcesConfig config, string source)
        {
            var prop = config.GetType().GetProperty(source);
            return (bool) prop.GetValue(config);
        }

        public static string GetSourceName(this string prefix)
        {
            prefix.Sub(0, prefix.Length - 5);
            return prefix switch
            {
                //"apm" => ("AppleMusic", typeof(AppleMusicSource)),
                //"am"  => ("Audiomack", typeof(AudiomackSource)),
                "bc" => "BandCamp",
                //"ht"  => ("HTTP", typeof(HttpSource)),
                //"lcl" => ("Local", typeof(LocalSource)),
                //"mxc" => ("MixCloud", typeof(MixCloudSource)),
                //"mx"  => ("Mixer", typeof(MixerSource)),
                //"mb"  => ("MusicBed", typeof(MusicBedSource)),
                "sc" => "SoundCloud",
                //"tw"  => ("Twitch", typeof(TwitchSource)),
                //"vm"  => ("Vimeo", typeof(VimeoSource)),
                // "yt"  => ("YouTube", typeof(YouTubeSource)),
                _ => "Unknown"
            };
        }

        public static async Task SendAsync(this WebSocket socket, object obj)
        {
            var serialize = obj.Serialize();
            await socket.SendAsync(serialize, WebSocketMessageType.Text, true, CancellationToken.None)
                .ConfigureAwait(false);
        }

        public static string GetString(this ReadOnlyMemory<byte> bytes)
        {
            var str = Encoding.UTF8.GetString(bytes.Span);
            return str;
        }

        public static void TrimEnd(ref byte[] array)
        {
            var lastIndex = Array.FindLastIndex(array, b => b != 0);
            Array.Resize(ref array, lastIndex + 1);
        }

        public static string WithPath(this string str, string path)
        {
            return $"{str}/{path}";
        }

        public static string WithParameter(this string str, string key, string value)
        {
            return str.Contains("?")
                ? str + $"&{key}={value}"
                : str + $"?{key}={value}";
        }

        public static Uri ToUrl(this string str)
        {
            return new Uri(str);
        }

        public static T As<T>(this object obj)
        {
            return obj is T value
                ? value
                : default;
        }

        public static SearchResponse VerifyResponse(this SearchResponse response)
        {
            response.Status = response.LoadType switch
            {
                LoadType.NoMatches   => Status.Error("Failed to find any result for your query."),
                LoadType.SearchError => Status.Error("Source didn't return any thing."),
                _                    => Status.Ok
            };

            response.Status = response.Tracks.Count == 0
                ? Status.Error("Failed to find any results for your query.")
                : Status.Ok;

            return response;
        }

        public static string Sub(this string str, int start, int length)
        {
            var span = str.AsSpan();
            span = span.Slice(start, length);
            return span.ToString();
        }
    }
}