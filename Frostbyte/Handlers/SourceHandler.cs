using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Frostbyte.Attributes;
using Frostbyte.Entities;
using Frostbyte.Entities.Enums;
using Frostbyte.Entities.Results;
using Frostbyte.Extensions;
using Frostbyte.Sources;
using Microsoft.Extensions.DependencyInjection;

namespace Frostbyte.Handlers
{
    [RegisterService]
    public sealed class SourceHandler
    {
        private readonly IEnumerable<ISourceProvider> _sources;

        public SourceHandler(IServiceProvider provider)
        {
            _sources = provider.GetServices<ISourceProvider>();
        }

        public async Task<ResponseEntity> HandlerRequestAsync(string prov, string query)
        {
            var source = _sources.FirstOrDefault(x => x.Prefix == prov);
            var response = new ResponseEntity(false, !source.IsEnabled ? $"{prov.GetSourceFromPrefix()} endpoint is disabled in config" : "Success");

            if (!source.IsEnabled)
                return response;

            response.IsSuccess = true;
            response.AdditionObject = await source.SearchAsync(query).ConfigureAwait(false);

            if (response.AdditionObject is SearchResult search && search.LoadType == LoadType.LoadFailed)
            {
                response.IsSuccess = false;
                response.Reason = "Source returned no result.";
            }

            return response;
        }
    }
}