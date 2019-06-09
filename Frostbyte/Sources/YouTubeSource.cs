using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Frostbyte.Attributes;
using Frostbyte.Entities;
using Frostbyte.Entities.Enums;
using Frostbyte.Entities.Results;
using Frostbyte.Extensions;
using Frostbyte.Handlers;

namespace Frostbyte.Sources
{
    [RegisterService(typeof(ISourceProvider))]
    public sealed class YouTubeSource : SourceCache, ISourceProvider
    {
        public string Prefix { get; }
        public bool IsEnabled { get; }

        private const string BASE_URL = "https://www.youtube.com";

        private readonly Regex _idRegex;

        public YouTubeSource(Configuration config)
        {
            Prefix = "ytsearch";
            IsEnabled = config.Sources.EnableYouTube;
            _idRegex = new Regex("(?!videoseries)[a-zA-Z0-9_-]{11,42}", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        }

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

            var get = await HttpHandler.Instance
                .GetBytesAsync(url).ConfigureAwait(false);

            switch (search.LoadType)
            {
                case LoadType.PlaylistLoaded:
                    GetPlaylist(get.Span, ref search, playlistId, query);
                    break;

                case LoadType.SearchResult:
                    GetSearch(get.Span, ref search);
                    break;

                case LoadType.TrackLoaded:
                    GetTrack(get.Span, videoId, ref search);
                    break;
            }

            return search;
        }

        public async ValueTask<Stream> GetStreamAsync(string id)
        {
            throw new System.NotImplementedException();
        }

        private void GetSearch(ReadOnlySpan<byte> span, ref SearchResult result)
        {
            var search = JsonSerializer.Parse<YouTubeSearch>(span);
            result.Tracks = search.Video.Select(x => x.ToTrack);
        }

        private void GetTrack(ReadOnlySpan<byte> span, string id, ref SearchResult result)
        {
            var search = JsonSerializer.Parse<YouTubeSearch>(span);
            var track = new[] { search.Video.FirstOrDefault(x => x.Id == id).ToTrack };
            result.Tracks = track;
        }

        private void GetPlaylist(ReadOnlySpan<byte> span, ref SearchResult result, string id, string url)
        {
            var playlist = JsonSerializer.Parse<YouTubePlaylist>(span);
            result.Playlist = playlist.BuildPlaylist(id, url);
            result.Tracks = playlist.Videos.Select(x => x.ToTrack);
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