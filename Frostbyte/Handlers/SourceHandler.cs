using System;
using System.Threading.Tasks;
using Frostbyte.Entities;
using Frostbyte.Entities.Enums;
using Frostbyte.Entities.Results;
using Frostbyte.Extensions;
using Frostbyte.Sources;

namespace Frostbyte.Handlers
{
    public sealed class SourceHandler
    {
        public async Task<ResponseEntity> HandleRequestAsync(string prefix, string query, IServiceProvider provider)
        {
            var sourceInfo = prefix.GetSourceInfo();
            var source = provider.GetService(sourceInfo.SourceType).TryCast<SourceBase>();

            var isEnabled = Singletons.Config.Sources.IsSourceEnabled($"Enable{sourceInfo.Name}");
            var response = new ResponseEntity(isEnabled, isEnabled ? $"{sourceInfo.Name} source is disable in configuration" : "Success");

            if (!isEnabled)
                return response;

            response.AdditionObject = await source.SearchAsync(query).ConfigureAwait(false);
            var searchResult = response.AdditionObject.TryCast<SearchResult>();

            response.IsSuccess = searchResult.LoadType == LoadType.LoadFailed || searchResult.LoadType == LoadType.NoMatches;
            response.Reason = searchResult.LoadType == LoadType.LoadFailed ?
                 $"{sourceInfo.Name} was unable to load anything" :
                searchResult.LoadType == LoadType.NoMatches ?
                $"{sourceInfo.Name} was failed to find any matches for {query}"
                : "Success";

            return response;
        }
    }
}