using Microsoft.AspNetCore.Mvc;

namespace Concept.Controllers
{
    [ApiController, Route("/ping")]
    public sealed class PingController : ControllerBase
    {
        private readonly Settings _settings;

        public PingController(Settings settings)
        {
            _settings = settings;
        }

        [HttpGet]
        public IActionResult Ping()
        {
            return Ok($"{_settings.Authorization}");
        }
    }
}