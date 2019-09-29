using Concept.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
        public IActionResult YouTube(string query)
        {
            return NotFound("Under construction.");
        }

        [HttpGet("soundcloud")]
        public IActionResult SoundCloud(string query)
        {
            return Ok();
        }
    }
}