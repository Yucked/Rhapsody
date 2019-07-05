using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Frostbyte.Entities.Audio;
using Frostbyte.Entities.Enums;
using Frostbyte.Entities.Responses;

namespace Frostbyte.Sources
{
    public sealed class LocalSource : BaseSourceProvider
    {
        public override ValueTask<SearchResponse> SearchAsync(string query)
        {
            var response = new SearchResponse();

            if (Directory.Exists(query))
            {
                var files = Directory
                    .EnumerateFiles(query, @"\.(?:wav|mp3|flac|m4a|ogg|wma|webm)$", SearchOption.AllDirectories)
                    .ToArray();
                if (files.Length < 1) return new ValueTask<SearchResponse>(response);

                response.Tracks = files.Select(x => BuildTrack(x));
                response.LoadType = LoadType.SearchResult;
            }
            else
            {
                var track = new[] {BuildTrack(query)};
                response.Tracks = track;
                response.LoadType = LoadType.TrackLoaded;
            }

            return new ValueTask<SearchResponse>(response);
        }

        protected override async ValueTask<Stream> GetStreamAsync(string query)
        {
            if (Directory.Exists(query))
                return default;

            using var stream = new FileStream(query, FileMode.Open);
            var memStream = new MemoryStream((int) stream.Length);
            await stream.CopyToAsync(memStream).ConfigureAwait(false);
            return memStream;
        }

        private AudioTrack BuildTrack(string filePath)
        {
            /*
            using var file = TagLib.File.Create(filePath);
            var track = new AudioTrack
            {
                Id = file.Name,
                Title = file.Tag.Title,
                Author = new TrackAuthor
                {
                    Name = file.Tag.FirstAlbumArtist
                },
                Duration = (int)file.Properties.Duration.TotalMilliseconds
            };

            return track;
            */
            return default;
        }
    }
}