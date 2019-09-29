using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Concept.Controllers
{
    // if you put the tag [controller] asp.net core will handle that with the name ahead controller, in this case /Tracks (ignore case).
    [ApiController, Route("[controller]"), Authorize]
    public sealed class TracksController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetTrack(string provider, string query)
        {
            return NotFound("Under construction.");
        }
    }
}