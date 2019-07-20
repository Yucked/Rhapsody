using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Frostbyte.Entities;
using Frostbyte.Entities.Enums;
using Frostbyte.Entities.Infos;
using Frostbyte.Entities.Results;

namespace Frostbyte.Sources
{
    public sealed class BandCampSource : BaseSource
    {
        private readonly Regex
            _trackUrlRegex = new Regex("^https?://(?:[^.]+\\.|)bandcamp\\.com/track/([a-zA-Z0-9-_]+)/?(?:\\?.*|)$",
                RegexOptions.Compiled | RegexOptions.IgnoreCase),
            _albumUrlRegex = new Regex("^https?://(?:[^.]+\\.|)bandcamp\\.com/album/([a-zA-Z0-9-_]+)/?(?:\\?.*|)$",
                RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public override async ValueTask<SearchResponse> SearchAsync(string query)
        {
            var response = new SearchResponse();
            query = query switch
            {
                var trackUrl when _trackUrlRegex.IsMatch(query) => trackUrl,
                var albumUrl when _albumUrlRegex.IsMatch(query) => albumUrl,
                _ =>
                $"https://bandcamp.com/search?q={WebUtility.UrlEncode(query)}"
            };

            var json = await ScrapeJsonAsync(query).ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(json))
            {
                response.LoadType = LoadType.SearchError;
                return response;
            }

            var bcResult = JsonSerializer.Deserialize<BandCampResult>(json);
            response.LoadType = bcResult.ItemType switch
            {
                "album" => LoadType.PlaylistLoaded,
                "track" => LoadType.TrackLoaded,
                _       => LoadType.NoMatches
            };

            if (response.LoadType is LoadType.NoMatches)
                return response;

            long duration = 0;
            foreach (var trackInfo in bcResult.Trackinfo)
            {
                var track = trackInfo.ToTrackInfo(bcResult.Artist, bcResult.Url, bcResult.ArtId);
                response.Tracks.Add(track);
                duration += track.Duration;
            }

            var playlistInfo = new PlaylistInfo($"{bcResult.Current.Id}", bcResult.Current.Title, bcResult.Url, duration,
                bcResult.ArtId == 0
                    ? default
                    : $"https://f4.bcbits.com/img/a{bcResult.ArtId}_0.jpg");

            response.Playlist = playlistInfo;

            return response;
        }

        public override async ValueTask<Stream> GetStreamAsync(string trackId)
        {
            if (!_trackUrlRegex.IsMatch(trackId))
                return default;

            var json = await ScrapeJsonAsync(trackId).ConfigureAwait(false);
            var bcResult = JsonSerializer.Deserialize<BandCampResult>(json);

            var track = bcResult.Trackinfo.FirstOrDefault();
            if (track is null)
                return default;

            var stream = await HttpFactory
                .GetStreamAsync(track.File.Mp3Url)
                .ConfigureAwait(false);

            return stream;
        }

        private async ValueTask<string> ScrapeJsonAsync(string url)
        {
            var rawHtml = await HttpFactory
                .GetStringAsync(url)
                .ConfigureAwait(false);

            if (string.IsNullOrWhiteSpace(rawHtml))
                return string.Empty;

            const string startStr = "var TralbumData = {",
                endStr = "};";

            if (rawHtml.IndexOf(startStr, StringComparison.Ordinal) == -1)
                return string.Empty;

            var tempData = rawHtml.Substring(rawHtml.IndexOf(startStr, StringComparison.Ordinal) + startStr.Length - 1);
            tempData = tempData.Substring(0, tempData.IndexOf(endStr, StringComparison.Ordinal) + 1);

            var jsonReg = new Regex(@"([a-zA-Z0-9_]*:\s)(?!\s)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            var commentReg = new Regex(@"\/\*[\s\S]*?\*\/|([^:]|^)\/\/.*",
                RegexOptions.Compiled | RegexOptions.IgnoreCase);

            tempData = commentReg.Replace(tempData, "");
            var matches = jsonReg.Matches(tempData);
            foreach (Match match in matches)
            {
                var val = $"\"{match.Value.Replace(": ", "")}\":";
                var regex = new Regex(Regex.Escape(match.Value), RegexOptions.Compiled | RegexOptions.IgnoreCase);
                tempData = regex.Replace(tempData, val, 1);
            }

            tempData = tempData.Replace("\" + \"", "");
            return tempData;
        }
    }
}