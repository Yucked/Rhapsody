using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Frostbyte.Entities;
using Frostbyte.Entities.Audio;
using Frostbyte.Entities.Enums;
using Frostbyte.Entities.Responses;
using Frostbyte.Extensions;
using Frostbyte.Sources;

namespace Frostbyte.Handlers
{
    public sealed class SourceHandler
    {
        private AudioSources _sources;
        private CacheHandler _cache;

        public void Initialize()
        {
            var matches = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(x => typeof(BaseSourceProvider).IsAssignableFrom(x) && !x.IsAbstract)
                .ToArray();

            LogHandler<SourceHandler>.Log.Debug($"Discovered {matches.Length} SourceProviders.");
            foreach (var match in matches)
                Singleton.Add(match);

            _sources = Singleton.Of<Configuration>().Sources;
            _cache = Singleton.Of<CacheHandler>();
        }

        public async Task<(bool IsEnabled, SearchResponse Response)> HandleRequestAsync(string prefix, string query)
        {
            var (name, sourceType) = prefix.GetSourceInfo();
            var source = Singleton.Of<BaseSourceProvider>(sourceType);
            var isEnabled = _sources.IsSourceEnabled($"Enable{name}");
            var result = await source.SearchAsync(query).ConfigureAwait(false);

            if (isEnabled && result.LoadType != (LoadType.LoadFailed | LoadType.NoMatches))
                _cache.Add(result.Tracks);

            return (isEnabled, result);
        }

        public ValueTask<Stream> GetStreamAsync(AudioTrack track)
        {
            var sourceInfo = track.Provider.GetSourceInfo();
            var source = Singleton.Of<BaseSourceProvider>(sourceInfo.SourceType);

            return source.GetStreamAsync(track);
        }
    }
}