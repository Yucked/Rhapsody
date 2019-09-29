using Microsoft.AspNetCore.Mvc;

namespace Concept.Controllers
{
    [ApiController, Route("/tracks")]
    public sealed class TracksController : ControllerBase
    {
        
        [HttpGet]
        public IActionResult GetTrack(string provider, string query)
        {
            return NotFound("Under construction.");
        }
    }
}