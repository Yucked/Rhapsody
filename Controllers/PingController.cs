using Microsoft.AspNetCore.Mvc;

namespace Concept.Controllers
{
    [ApiController, Route("/ping")]
    public sealed class PingController : ControllerBase
    {
        [HttpGet]
        public IActionResult Ping()
            => Ok();
    }
}