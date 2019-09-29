using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Concept.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ValuesController : ControllerBase
    {
        public ValuesController()
        {
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Ok("Hello World");
        }
    }
}