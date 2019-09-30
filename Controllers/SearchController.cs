using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Theory;
using Theory.Providers;

namespace Concept.Controllers
{
    [ApiController, Route("/search"), Authorize]
    public sealed class SearchController : ControllerBase
    {
        private readonly Theoretical _theoretical;

        public SearchController(Theoretical theoretical)
            => _theoretical = theoretical;

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

            var provider = _theoretical.GetProvider(providerType);
            var result = await provider.SearchAsync(query);

            return Ok(result);
        }
    }
}