using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Concept.Controllers
{
    [ApiController, Authorize, Route("/search")]
    public sealed class SearchController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetTrack(string provider, string query)
        {
            return NotFound("Under construction.");
        }
    }
}