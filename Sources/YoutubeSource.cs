using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Frostbyte.Entities.Enums;
using Frostbyte.Entities.Results;
using Frostbyte.Extensions;
using Frostbyte.Handlers;

namespace Frostbyte.Sources
{
    public sealed class YouTubeSource : ISourceProvider
    {
        private const string BASE_URL = "https://www.youtube.com";

        private readonly Regex _idRegex
            = new Regex("(?!videoseries)[a-zA-Z0-9_-]{11,42}", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public async ValueTask<SearchResult> SearchAsync(string query)
        {
            var search = new SearchResult();
            var url = string.Empty;
            TryParseId(query, out var videoId, out var playlistId);

            switch (Uri.IsWellFormedUriString(query, UriKind.RelativeOrAbsolute))
            {
                case true:
                    if (query.Contains("list="))
                    {
                        url = BASE_URL
                            .WithPath("list_ajax")
                            .WithParameter("style", "json")
                            .WithParameter("action_get_list", "1")
                            .WithParameter("list", playlistId);

                        search.LoadType = LoadType.PlaylistLoaded;
                    }
                    else
                    {
                        url = BASE_URL
                            .WithPath("search_ajax")
                            .WithParameter("style", "json")
                            .WithParameter("search_query", WebUtility.UrlEncode(query));

                        search.LoadType = LoadType.TrackLoaded;
                    }

                    break;

                case false:
                    url = BASE_URL
                        .WithPath("search_ajax")
                        .WithParameter("style", "json")
                        .WithParameter("search_query", WebUtility.UrlEncode(query));

                    search.LoadType = LoadType.SearchResult;
                    break;
            }

            var get = await Singleton.Of<HttpHandler>()
                .GetBytesAsync(url).ConfigureAwait(false);

            if (get.IsEmpty)
            {
                search.LoadType = LoadType.LoadFailed;
                return search;
            }

            switch (search.LoadType)
            {
                case LoadType.PlaylistLoaded:
                    var playlist = JsonSerializer.Parse<YouTubePlaylist>(get.Span);
                    search.Playlist = playlist.BuildPlaylist(playlistId, url);
                    search.Tracks = playlist.Videos.Select(x => x.ToTrack);
                    break;

                case LoadType.SearchResult:
                    var ytSearch = JsonSerializer.Parse<YouTubeSearch>(get.Span);
                    search.Tracks = ytSearch.Video.Select(x => x.ToTrack);
                    break;

                case LoadType.TrackLoaded:
                    ytSearch = JsonSerializer.Parse<YouTubeSearch>(get.Span);
                    search.Tracks = new[] { ytSearch.Video.FirstOrDefault(x => x.Id == videoId).ToTrack };
                    break;
            }

            return search;
        }

        public async ValueTask<Stream> GetStreamAsync(string query)
        {
            if (query.Length != 11)
                return default;

            throw new System.NotImplementedException();
        }

        private bool TryParseId(string url, out string videoId, out string playlistId)
        {
            var matches = _idRegex.Matches(url);
            var (vidId, plyId) = ("", "");

            foreach (Match match in matches)
            {
                if (!match.Success)
                    continue;

                if (match.Length == 11)
                    vidId = match.Value;
                else
                    plyId = match.Value;
            }

            videoId = vidId;
            playlistId = plyId;

            return matches.Count == 0;
        }
    }
}