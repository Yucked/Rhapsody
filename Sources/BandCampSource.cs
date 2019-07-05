using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Frostbyte.Entities.Enums;
using Frostbyte.Entities.Responses;
using Frostbyte.Entities.Results;
using Frostbyte.Handlers;

namespace Frostbyte.Sources
{
    public sealed class BandCampSource : BaseSourceProvider
    {
        private readonly Regex
            _trackUrlRegex = new Regex("^https?://(?:[^.]+\\.|)bandcamp\\.com/track/([a-zA-Z0-9-_]+)/?(?:\\?.*|)$",
                RegexOptions.Compiled | RegexOptions.IgnoreCase),
            _albumUrlRegex = new Regex("^https?://(?:[^.]+\\.|)bandcamp\\.com/album/([a-zA-Z0-9-_]+)/?(?:\\?.*|)$",
                RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public override async ValueTask<SearchResponse> SearchAsync(string query)
        {
            var result = new SearchResponse();
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
                result.LoadType = LoadType.LoadFailed;
                return result;
            }

            var bcResult = JsonSerializer.Parse<BandCampResult>(json);
            result.LoadType = bcResult.ItemType == "album" ? LoadType.PlaylistLoaded :
                bcResult.ItemType == "track" ? LoadType.TrackLoaded : LoadType.NoMatches;

            if (result.LoadType is LoadType.LoadFailed)
                return result;

            result.Playlist = bcResult.ToAudioPlaylist;
            result.Tracks = bcResult.Tracks;
            return result;
        }

        protected override async ValueTask<Stream> GetStreamAsync(string query)
        {
            if (!_trackUrlRegex.IsMatch(query))
                return default;

            var json = await ScrapeJsonAsync(query).ConfigureAwait(false);
            var bcResult = JsonSerializer.Parse<BandCampResult>(json);

            var track = bcResult.Trackinfo.FirstOrDefault();
            if (track is null)
                return default;

            var stream = await Singleton.Of<HttpHandler>()
                .GetStreamAsync(track.File.Mp3Url).ConfigureAwait(false);
            return stream;
        }

        private async ValueTask<string> ScrapeJsonAsync(string url)
        {
            var rawHtml = await Singleton.Of<HttpHandler>()
                .GetStringAsync(url).ConfigureAwait(false);
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