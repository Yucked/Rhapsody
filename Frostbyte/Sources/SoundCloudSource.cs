using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
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
    public sealed class SoundCloudSource : SourceCache, ISourceProvider
    {
        public string Prefix { get; }
        public bool IsEnabled { get; }
        private const string
            BASE_URL = "https://api.soundcloud.com",
            CLIENT_ID = "a3dd183a357fcff9a6943c0d65664087",
            PATTERN_SCRIPT = "https://[A-Za-z0-9-.]+/assets/app-[a-f0-9-]+\\.js",
            PATTERN_CLIENT_ID = "/,client_id:\"([a-zA-Z0-9-_]+)\"/",
            PATTERN_TRACK = @"^(https?:\/\/)?(www.)?(m\.)?(soundcloud\.com|snd\.sc)\/?([a-zA-Z0-9-_]+)\/?([a-zA-Z0-9-_]+)$",
            PATTERN_PLAYLIST = @"^(https?:\/\/)?(www.)?(m\.)?(soundcloud\.com|snd\.sc)\/?([a-zA-Z0-9-_]+)\/(sets+)\/?([a-zA-Z0-9-_]+)$";

        public SoundCloudSource(Configuration config)
        {
            Prefix = "scsearch";
            IsEnabled = config.Sources.EnableSoundCloud;
        }

        public async ValueTask<SearchResult> SearchAsync(string query)
        {
            var response = new SearchResult();
            var url = string.Empty;

            switch (query)
            {
                case var q when Uri.IsWellFormedUriString(query, UriKind.RelativeOrAbsolute):
                    if (!q.Contains("sets"))
                    {
                        url = BASE_URL
                              .WithPath("resolve")
                              .WithParameter("url", query)
                              .WithParameter("client_id", CLIENT_ID);

                        response.LoadType = LoadType.TrackLoaded;
                    }
                    else
                    {
                        url = BASE_URL
                            .WithPath("resolve")
                            .WithParameter("url", query)
                            .WithParameter("client_id", CLIENT_ID);

                        response.LoadType = LoadType.PlaylistLoaded;
                    }
                    break;

                case var _ when !Uri.IsWellFormedUriString(query, UriKind.RelativeOrAbsolute):
                    url = BASE_URL
                        .WithPath("tracks")
                        .WithParameter("q", query)
                        .WithParameter("client_id", CLIENT_ID);

                    response.LoadType = LoadType.SearchResult;
                    break;
            }

            var get = await HttpHandler.Instance.GetBytesAsync(url).ConfigureAwait(false);
            if (get.IsEmpty)
            {
                response.LoadType = LoadType.LoadFailed;
                return response;
            }

            switch (response.LoadType)
            {
                case LoadType.TrackLoaded:
                    GetTrack(get.Span, ref response);
                    break;

                case LoadType.PlaylistLoaded:
                    GetPlaylist(get.Span, ref response);
                    break;

                case LoadType.SearchResult:
                    GetSearch(get.Span, ref response);
                    break;
            }

            return response;
        }

        public async ValueTask<Stream> GetStreamAsync(string id)
        {
            var get = await HttpHandler.Instance
                .WithUrl($"{BASE_URL}/tracks/{id}/stream")
                .WithParameter("client_id", CLIENT_ID)
                .GetBytesAsync().ConfigureAwait(false);

            if (get.IsEmpty)
            {
                return default;
            }

            var read = JsonSerializer.Parse<SoundCloudDirectUrl>(get.Span);
            var stream = await HttpHandler.Instance
                .WithUrl(read.Url)
                .GetStreamAsync().ConfigureAwait(false);
            return stream;
        }

        private void GetSearch(ReadOnlySpan<byte> bytes, ref SearchResult result)
        {
            var parse = JsonSerializer.Parse<IEnumerable<SoundCloudTrack>>(bytes);
            result.Tracks = parse.Select(x => x.ToTrack);
        }

        private void GetTrack(ReadOnlySpan<byte> bytes, ref SearchResult result)
        {
            var parse = JsonSerializer.Parse<SoundCloudTrack>(bytes);
            var tracks = new[] { parse.ToTrack };
            result.Tracks = tracks;
        }

        private void GetPlaylist(ReadOnlySpan<byte> bytes, ref SearchResult result)
        {
            var parse = JsonSerializer.Parse<SoundCloudPlaylist>(bytes);
            result.Playlist = parse.ToPlaylist;
            result.Tracks = parse.Tracks.Select(x => x.ToTrack);
        }
    }
}