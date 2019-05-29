using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text.Utf8;
using System.Threading.Tasks;

namespace Frostbyte.Handlers
{
    public sealed class HttpHandler
    {
        private static readonly Lazy<HttpHandler> LazyHelper = new Lazy<HttpHandler>(() => new HttpHandler());

        private HttpClient _client;

        public static HttpHandler Instance => LazyHelper.Value;

        private void CheckClient()
        {
            if (!(_client is null))
                return;

            _client = new HttpClient(new HttpClientHandler
            {
                UseCookies = false,
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            });
            _client.DefaultRequestHeaders.Clear();
            _client.DefaultRequestHeaders.Add("User-Agent", "Frostbyte");
        }

        public async ValueTask<ReadOnlyMemory<byte>> GetBytesAsync(string url)
        {
            CheckClient();

            var get = await _client.GetAsync(url).ConfigureAwait(false);
            if (!get.IsSuccessStatusCode)
                return default;

            using var content = get.Content;
            await using var readStream = await content.ReadAsStreamAsync().ConfigureAwait(false);
            readStream.Position = 0;
            var readOnly = new ReadOnlyMemory<byte>();
            await readStream.WriteAsync(readOnly).ConfigureAwait(false);
            return readOnly;
        }

        public HttpHandler WithCustomHeader(string key, string value)
        {
            CheckClient();

            if (_client.DefaultRequestHeaders.Contains(key))
            {
                return this;
            }

            _client.DefaultRequestHeaders.Add(key, value);
            {
                return this;
            }
        }
    }
}