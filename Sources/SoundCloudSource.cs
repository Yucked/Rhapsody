using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Frostbyte.Entities.Enums;
using Frostbyte.Entities.Responses;
using Frostbyte.Entities.Results;
using Frostbyte.Extensions;
using Frostbyte.Handlers;

namespace Frostbyte.Sources
{
    public sealed class SoundCloudSource : BaseSourceProvider
    {
        private const string
            BASE_URL = "https://api.soundcloud.com",
            CLIENT_ID = "a3dd183a357fcff9a6943c0d65664087",
            PATTERN_SCRIPT = "https://[A-Za-z0-9-.]+/assets/app-[a-f0-9-]+\\.js",
            PATTERN_CLIENT_ID = "/,client_id:\"([a-zA-Z0-9-_]+)\"/",
            PATTERN_TRACK =
                @"^(https?:\/\/)?(www.)?(m\.)?(soundcloud\.com|snd\.sc)\/?([a-zA-Z0-9-_]+)\/?([a-zA-Z0-9-_]+)$",
            PATTERN_PLAYLIST =
                @"^(https?:\/\/)?(www.)?(m\.)?(soundcloud\.com|snd\.sc)\/?([a-zA-Z0-9-_]+)\/(sets+)\/?([a-zA-Z0-9-_]+)$";

        public override async ValueTask<SearchResponse> SearchAsync(string query)
        {
            var result = new SearchResponse();
            var url = string.Empty;

            switch (query)
            {
                case var q when Uri.IsWellFormedUriString(query, UriKind.Absolute):
                    if (!q.Contains("sets"))
                    {
                        url = BASE_URL
                            .WithPath("resolve")
                            .WithParameter("url", query)
                            .WithParameter("client_id", CLIENT_ID);

                        result.LoadType = LoadType.TrackLoaded;
                    }
                    else
                    {
                        url = BASE_URL
                            .WithPath("resolve")
                            .WithParameter("url", query)
                            .WithParameter("client_id", CLIENT_ID);

                        result.LoadType = LoadType.PlaylistLoaded;
                    }

                    break;

                case var _ when !Uri.IsWellFormedUriString(query, UriKind.Absolute):
                    url = BASE_URL
                        .WithPath("tracks")
                        .WithParameter("q", query)
                        .WithParameter("client_id", CLIENT_ID);

                    result.LoadType = LoadType.SearchResult;
                    break;
            }

            var bytes = await Singleton.Of<HttpHandler>().GetBytesAsync(url).ConfigureAwait(false);
            if (bytes.IsEmpty)
            {
                result.LoadType = LoadType.LoadFailed;
                return result;
            }

            switch (result.LoadType)
            {
                case LoadType.TrackLoaded:
                    var scTrack = JsonSerializer.Parse<SoundCloudTrack>(bytes.Span);
                    var tracks = new[] { scTrack.ToTrack };
                    result.Tracks = tracks;
                    break;

                case LoadType.PlaylistLoaded:
                    var scPly = JsonSerializer.Parse<SoundCloudPlaylist>(bytes.Span);
                    result.Playlist = scPly.ToPlaylist;
                    result.Tracks = scPly.Tracks.Select(x => x.ToTrack);
                    break;

                case LoadType.SearchResult:
                    var scTracks = JsonSerializer.Parse<IEnumerable<SoundCloudTrack>>(bytes.Span);
                    result.Tracks = scTracks.Select(x => x.ToTrack);
                    break;
            }

            return result;
        }

        protected override async ValueTask<Stream> GetStreamAsync(string query)
        {
            var bytes = await Singleton.Of<HttpHandler>()
                .WithUrl(BASE_URL)
                .WithPath("tracks")
                .WithPath(query)
                .WithPath("stream")
                .WithParameter("client_id", CLIENT_ID)
                .GetBytesAsync().ConfigureAwait(false);

            if (bytes.IsEmpty) return default;

            var read = JsonSerializer.Parse<SoundCloudDirectUrl>(bytes.Span);
            var stream = await Singleton.Of<HttpHandler>()
                .WithUrl(read.Url)
                .GetStreamAsync().ConfigureAwait(false);
            return stream;
        }
    }
}