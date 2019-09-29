using Concept.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Theory.Providers;

namespace Concept.Controllers
{
    [ApiController, Route("/search"), Authorize]
    public sealed class SearchController : ControllerBase
    {
        private readonly TheoryService _theory;

        public SearchController(TheoryService theory)
        {
            _theory = theory;
        }

        [HttpGet("youtube")]
        public async Task<IActionResult> YouTube(string query)
            => await _theory.SearchFor(ProviderType.YouTube, query);

        [HttpGet("soundcloud")]
        public async Task<IActionResult> SoundCloud(string query)
            => await _theory.SearchFor(ProviderType.SoundCloud, query);

        [HttpGet("bandcamp")]
        public async Task<IActionResult> Bandcamp(string query)
            => await _theory.SearchFor(ProviderType.BandCamp, query);
    }
}