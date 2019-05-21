using System;
using System.Net;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Frostbyte.Extensions
{
    public static class MiscExtensions
    {
        public static ValueTask SendResponseAsync(this HttpListenerContext context, object @object)
        {
            return context.Response.OutputStream.WriteAsync(JsonSerializer.ToBytes(@object));
        }
    }
}