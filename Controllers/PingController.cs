using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Concept.Controllers
{
    [ApiController, Route("/ping"), AllowAnonymous]
    public class PingController : ControllerBase
    {
        // This endpoint is to check if our server is alive, very useful for monitoring services like updown.io
        // https://updown.io/
        [HttpGet]
        public IActionResult Ping()
            => Ok();
    }
}