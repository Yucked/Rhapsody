using Frostbyte.Entities;
using Frostbyte.Handlers;
using System;

namespace Frostbyte
{
    public sealed class Singletons
    {
        private static Lazy<HttpHandler> _lazyHttp
            => new Lazy<HttpHandler>(() => new HttpHandler(), true);

        private static Lazy<CacheHandler> _lazyCache
            = new Lazy<CacheHandler>(() => new CacheHandler(), true);

        public static HttpHandler Http => _lazyHttp.Value;

        public static CacheHandler Cache => _lazyCache.Value;

        public static Configuration Config { get; private set; }

        public static void SetConfig(Configuration config)
        {
            Config = config;
        }
    }
}