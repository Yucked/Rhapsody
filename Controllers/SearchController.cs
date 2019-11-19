using System;
using System.Threading.Tasks;
using Concept.Caches;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Theory;
using Theory.Providers;

namespace Concept.Controllers
{
    [ApiController, Route("/search"), Authorize]
    public sealed class SearchController : ControllerBase
    {
        private readonly Theoretical _theoretical;
        private readonly ResponsesCache _cache;

        public SearchController(Theoretical theoretical, IServiceProvider serviceProvider)
        {
            _theoretical = theoretical;
            _cache = serviceProvider.GetService<ResponsesCache>();
        }

        [HttpGet("youtube")]
        public Task<IActionResult> SearchYouTubeAsync(string query)
            => SearchAsync(ProviderType.YouTube, query);

        [HttpGet("soundcloud")]
        public Task<IActionResult> SearchSoundCloudAsync(string query)
            => SearchAsync(ProviderType.SoundCloud, query);

        [HttpGet("bandcamp")]
        public Task<IActionResult> SearchBandCampAsync(string query)
            => SearchAsync(ProviderType.BandCamp, query);

        private async Task<IActionResult> SearchAsync(ProviderType providerType, string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return BadRequest("Missing query parameter.");

            if (_cache != null &&
                _cache.TryGetCache(query, out var response, providerType))
                return Ok(response);

            var provider = _theoretical.GetProvider(providerType);
            var result = await provider.SearchAsync(query);

            _cache?.TryAddCache(result, providerType);

            return Ok(result);
        }
    }
}