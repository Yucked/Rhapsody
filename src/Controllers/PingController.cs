using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Rhapsody.Payloads.Outbound;

namespace Rhapsody.Controllers {
	[Route("api/[controller]"), ApiController, Produces("application/json")]
	public sealed class PingController : ControllerBase {
		private readonly IMemoryCache _memoryCache;

		public PingController(IMemoryCache memoryCache) {
			_memoryCache = memoryCache;
		}

		[HttpGet]
		public IActionResult Ping() {
			if (_memoryCache.TryGetValue("PING", out DateTimeOffset old)) {
				return RestResponse.Ok(old);
			}

			old = DateTimeOffset.UtcNow;
			_memoryCache.Set("PING", old);
			return RestResponse.Ok(old);
		}
	}
}