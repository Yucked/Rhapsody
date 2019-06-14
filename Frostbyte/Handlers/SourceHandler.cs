using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Frostbyte.Entities;
using Frostbyte.Entities.Audio;
using Frostbyte.Entities.Enums;
using Frostbyte.Entities.Results;
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
                .Where(x => typeof(ISourceProvider).IsAssignableFrom(x) && !x.IsInterface)
                .ToArray();

            LogHandler<SourceHandler>.Log.Debug($"Discovered {matches.Length} SourceProviders.");
            foreach (var match in matches)
                Singleton.Add(match);

            _sources = Singleton.Of<Configuration>().Sources;
        }

        public async Task<ResponseEntity> HandleRequestAsync(string prefix, string query)
        {
            var sourceInfo = prefix.GetSourceInfo();
            var source = Singleton.Of<ISourceProvider>(sourceInfo.SourceType);

            var isEnabled = _sources.IsSourceEnabled($"Enable{sourceInfo.Name}");
            var response = new ResponseEntity(isEnabled, !isEnabled ? $"{sourceInfo.Name} source is disable in configuration" : "Success");

            if (!isEnabled)
                return response;

            response.AdditionObject = await source.SearchAsync(query).ConfigureAwait(false);
            var searchResult = response.AdditionObject.TryCast<SearchResult>();

            response.IsSuccess = searchResult.LoadType == LoadType.LoadFailed || searchResult.LoadType == LoadType.NoMatches;
            response.Reason =
                searchResult.LoadType == LoadType.LoadFailed ?
                 $"{sourceInfo.Name} was unable to load anything for {query}" :
                searchResult.LoadType == LoadType.NoMatches ?
                $"{sourceInfo.Name} failed to find any matches for {query}"
                : "Success";

            return response;
        }

        public Task<Stream> GetStreamAsync(string provider, AudioTrack track)
        {
            var sourceInfo = provider.GetSourceInfo();
            var source = Singleton.Of<ISourceProvider>(sourceInfo.SourceType);

            return source.GetStreamAsync(track);
        }
    }
}