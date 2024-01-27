using System;
using System.Threading.Tasks;
using Dysc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Rhapsody.Internals.Attributes;
using Rhapsody.Payloads.Outbound;

namespace Rhapsody.Controllers {
	[Route("api/[controller]"), ApiController, Produces("application/json")]
	[ServiceFilter(typeof(ProviderFilterAttribute))]
	public sealed class SearchController : ControllerBase {
		private readonly Dysk _dysk;
		private readonly IMemoryCache _memoryCache;
		private readonly ILogger _logger;

		public SearchController(Dysk dysk, ILogger<SearchController> logger, IMemoryCache memoryCache) {
			_dysk = dysk;
			_logger = logger;
			_memoryCache = memoryCache;
		}


		[HttpGet("youtube")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		public ValueTask<IActionResult> GetYouTubeAsync(string query, bool isPlaylist = false) {
			return SearchAsync(SourceProvider.YouTube, query, isPlaylist);
		}

		[HttpGet("soundcloud")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		public ValueTask<IActionResult> GetSoundCloudAsync(string query, bool isPlaylist = false) {
			return SearchAsync(SourceProvider.SoundCloud, query, isPlaylist);
		}

		[HttpGet("bandcamp")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		public ValueTask<IActionResult> GetBandCampAsync(string query, bool isPlaylist = false) {
			return SearchAsync(SourceProvider.BandCamp, query, isPlaylist);
		}

		[HttpGet("hearthisat")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		public ValueTask<IActionResult> GetHearThisAtAsync(string query, bool isPlaylist = false) {
			return SearchAsync(SourceProvider.HearThisAt, query, isPlaylist);
		}

		[HttpGet("http")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		public ValueTask<IActionResult> GetHttpAsync(string query, bool isPlaylist = false) {
			return !Uri.IsWellFormedUriString(query, UriKind.Absolute)
				? new ValueTask<IActionResult>(RestResponse.Error("Query must be an absolute URI string."))
				: SearchAsync(SourceProvider.Http, query, isPlaylist);
		}

		private async ValueTask<IActionResult> SearchAsync(SourceProvider providerType, string query, bool isPlaylist) {
			if (string.IsNullOrWhiteSpace(query)) {
				return RestResponse.Error("Query must not be empty.");
			}

			try {
				var provider = _dysk.GetProvider(providerType);
				if (isPlaylist) {
					var playlistResult = await provider.GetPlaylistAsync(query);
					return RestResponse.Ok(playlistResult);
				}

				var trackResults = await provider.SearchAsync(query);
				return RestResponse.Ok(trackResults);
			}
			catch (Exception exception) {
				_logger.LogCritical(exception, exception.StackTrace);
				return RestResponse.Error(exception.Message);
			}
		}

		/*
		private bool TrySearchCache(SourceProvider providerType, string query, out SearchResponse searchResponse) {
			searchResponse = default;
			if (!_memoryCache.TryGetValue(providerType, out ICollection<SearchResponse> searchResponses)) {
				return false;
			}

			foreach (var response in searchResponses) {
				if (response.Query.IsFuzzyMatch(query)) {
					searchResponse = response;
					return true;
				}

				var sum = response.Tracks.Sum(info => {
					if (info.Url.IsFuzzyMatch(query)) {
						return 1;
					}

					if (info.Title.IsFuzzyMatch(query)) {
						return 1;
					}

					return $"{info.Author} {info.Title}".IsFuzzyMatch(query) ? 1 : 0;
				});

				if (sum / response.Tracks.Count <= 85) {
					continue;
				}

				searchResponse = response;
				return true;
			}

			return false;
		}
		*/
	}
}