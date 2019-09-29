using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Theory;
using Theory.Providers;
using Theory.Search;
using Config = Concept.Configuration.Configuration;

namespace Concept.Services
{
    public class TheoryService
    {
        private readonly Config _config;
        private readonly Theoretical _theory;

        public TheoryService(Config config)
        {
            _config = config;
            _theory = new Theoretical(default);
        }

        public async Task<IActionResult> SearchFor(ProviderType provider, string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return new BadRequestObjectResult("The query cannot be null or empty.");

            if (!SourceEnabled(provider))
                return new BadRequestObjectResult("This source is disabled.");

            var source = _theory.GetSource(provider);

            // Asp.Net Core is a console app or a windows service and don't need ConfigureAwait(false)
            // see https://stackoverflow.com/questions/13489065/best-practice-to-call-configureawait-for-all-server-side-code
            var search = await source.SearchAsync(query);

            if (search.Status == SearchStatus.NoMatches || search.Tracks.Count <= 0)
                return new NoContentResult();

            return new OkObjectResult(search);
        }

        private bool SourceEnabled(ProviderType provider)
        {
            return bool.Parse(_config.Sources
                ?.GetType()
                ?.GetProperty(provider.ToString())
                ?.GetValue(_config.Sources)
                ?.ToString());
        }
    }
}