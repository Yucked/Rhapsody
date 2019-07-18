using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Frostbyte.Entities;
using Frostbyte.Misc;
using Frostbyte.Sources;

namespace Frostbyte.Factories
{
    public sealed class SourceFactory
    {
        private readonly ConcurrentDictionary<string, BaseSource> _sources;
        private readonly SourcesConfig _sourcesConfig;

        public SourceFactory()
        {
            _sourcesConfig = Singleton.Of<Configuration>().Audio.Sources;
            _sources = new ConcurrentDictionary<string, BaseSource>();
        }

        public void CreateSources()
        {
            var matches = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(x => typeof(BaseSource).IsAssignableFrom(x)
                            && !x.IsAbstract)
                .ToArray();

            if (matches.Length == 0)
                return;

            foreach (var match in matches)
            {
                var source = Activator.CreateInstance(match).As<BaseSource>();
                _sources.TryAdd(match.Name.Sub(0, match.Name.Length - 6), source);
            }
        }

        public async Task<SearchResponse> ProcessRequestAsync(string provider, string query)
        {
            var name = provider.GetSourceName();

            if (!_sources.TryGetValue(name, out var source))
                return SearchResponse.WithError($"Invalid provider name given: {provider}");

            var isSourceEnabled = _sourcesConfig.IsSourceEnabled(name);

            if (!isSourceEnabled)
                return SearchResponse.WithError($"Source {name} is disabled in configuration.");

            var search = await source.SearchAsync(query)
                .ConfigureAwait(false);

            return search.VerifyResponse();
        }
    }
}