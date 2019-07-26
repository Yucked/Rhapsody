using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Frostbyte.Entities;
using Frostbyte.Entities.Infos;
using Frostbyte.Sources;

namespace Frostbyte.Factories
{
    public sealed class SourceFactory
    {
        private readonly ConcurrentDictionary<string, BaseSource> _sources;
        private readonly SourcesConfig _sourcesConfig;
        private readonly ConcurrentDictionary<string, TrackInfo> _tracks;

        public SourceFactory()
        {
            _sourcesConfig = Singleton.Of<Configuration>()
                .Audio.Sources;
            _sources = new ConcurrentDictionary<string, BaseSource>();
            _tracks = new ConcurrentDictionary<string, TrackInfo>();
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
                var source = Activator.CreateInstance(match)
                    .As<BaseSource>();
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

            var response = search.VerifyResponse();
            if (response.Tracks.Count == 0)
                return response;

            foreach (var track in response.Tracks.Where(track => !_tracks.ContainsKey(track.Id)))
                _tracks.TryAdd(track.Id, track);

            return response;
        }

        public async Task<Stream> GetStreamAsync(string provider, string trackId)
        {
            var name = provider.GetSourceName();
            if (!_sources.TryGetValue(name, out var source))
                return default;

            var stream = await source.GetStreamAsync(trackId)
                .ConfigureAwait(false);
            return stream;
        }

        public TrackInfo GetTrack(string id)
        {
            _tracks.TryGetValue(id, out var track);
            return track;
        }
    }
}