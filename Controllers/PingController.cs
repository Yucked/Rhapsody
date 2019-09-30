using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

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