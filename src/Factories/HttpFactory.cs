using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Frostbyte.Misc;

namespace Frostbyte.Factories
{
    public sealed class HttpFactory
    {
        private string Url { get; set; }
        private readonly HttpClient _client;

        public HttpFactory()
        {
            _client = new HttpClient(new HttpClientHandler
            {
                UseCookies = false,
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            });

            _client.DefaultRequestHeaders.Clear();
            _client.DefaultRequestHeaders.Add("User-Agent", "Frostbyte");
        }

        public HttpFactory WithUrl(string url)
        {
            Url = url;
            return this;
        }

        public HttpFactory WithPath(string path)
        {
            Url = Url.WithPath(path);
            return this;
        }

        public HttpFactory WithParameter(string key, string value)
        {
            Url = Url.WithParameter(key, value);
            return this;
        }

        public HttpFactory WithCustomHeader(string key, string value)
        {
            if (_client.DefaultRequestHeaders.Contains(key))
                return this;

            _client.DefaultRequestHeaders.Add(key, value);
            return this;
        }

        public async ValueTask<bool> PingAsync()
        {
            try
            {
                using var get = await _client.GetAsync("https://google.com")
                    .ConfigureAwait(false);
                return get.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async ValueTask<ReadOnlyMemory<byte>> GetBytesAsync(string url = default)
        {
            url ??= Url;
            if (string.IsNullOrWhiteSpace(url))
                return default;

            using var get = await _client.GetAsync(url).ConfigureAwait(false);
            if (!get.IsSuccessStatusCode)
            {
                LogFactory.Error<HttpFactory>($"{url} returned {get.ReasonPhrase}.");
                return default;
            }

            using var content = get.Content;
            var array = await content.ReadAsByteArrayAsync().ConfigureAwait(false);
            Url = string.Empty;

            return new ReadOnlyMemory<byte>(array);
        }

        public async ValueTask<Stream> GetStreamAsync(string url = default)
        {
            url ??= Url;
            if (string.IsNullOrWhiteSpace(url))
                return default;

            using var get = await _client.GetAsync(url).ConfigureAwait(false);
            if (!get.IsSuccessStatusCode)
            {
                LogFactory.Error<HttpFactory>($"{url} returned {get.ReasonPhrase}.");
                return default;
            }

            using var content = get.Content;
            var stream = await content.ReadAsStreamAsync().ConfigureAwait(false);
            Url = string.Empty;

            return stream;
        }

        public async ValueTask<string> GetStringAsync(string url = default)
        {
            url ??= Url;
            if (string.IsNullOrWhiteSpace(url))
                return default;

            using var get = await _client.GetAsync(url).ConfigureAwait(false);
            if (!get.IsSuccessStatusCode)
            {
                LogFactory.Error<HttpFactory>($"{url} returned {get.ReasonPhrase}.");
                return default;
            }

            using var content = get.Content;
            var str = await content.ReadAsStringAsync().ConfigureAwait(false);
            Url = string.Empty;

            return str;
        }
    }
}