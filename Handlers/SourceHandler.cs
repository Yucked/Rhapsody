using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Frostbyte.Entities;
using Frostbyte.Entities.Audio;
using Frostbyte.Entities.Responses;
using Frostbyte.Extensions;
using Frostbyte.Sources;

namespace Frostbyte.Handlers
{
    public sealed class SourceHandler
    {
        private AudioSources _sources;

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
        }

        public async Task<(bool IsEnabled, SearchResponse Response)> HandleRequestAsync(string prefix, string query)
        {
            var sourceInfo = prefix.GetSourceInfo();
            var source = Singleton.Of<BaseSourceProvider>(sourceInfo.SourceType);
            var isEnabled = _sources.IsSourceEnabled($"Enable{sourceInfo.Name}");
            var result = await source.SearchAsync(query).ConfigureAwait(false);
            return (isEnabled, result);
        }

        public ValueTask<Stream> GetStreamAsync(string provider, AudioTrack track)
        {
            var sourceInfo = provider.GetSourceInfo();
            var source = Singleton.Of<BaseSourceProvider>(sourceInfo.SourceType);

            return source.GetStreamAsync(track);
        }
    }
}