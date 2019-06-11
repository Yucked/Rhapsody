using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Frostbyte.Entities;
using Frostbyte.Entities.Audio;
using Frostbyte.Entities.Enums;
using Frostbyte.Entities.Results;

namespace Frostbyte.Sources
{
    
    public sealed class LocalSource : SourceBase
    {
        public override string Prefix { get; }

        public LocalSource(Configuration config) : base(config)
        {
            Prefix = "lclsearch";
        }

        public override ValueTask<SearchResult> SearchAsync(string query)
        {
            var response = new SearchResult();

            if (Directory.Exists(query))
            {
                var files = Directory.EnumerateFiles(query, @"\.(?:wav|mp3|flac|m4a|ogg|wma|webm)$", SearchOption.AllDirectories).ToArray();
                if (files.Length < 1)
                {
                    return new ValueTask<SearchResult>(response);
                }

                response.Tracks = files.Select(x => BuildTrack(x));

                response.LoadType = LoadType.SearchResult;
            }
            else
            {
                var track = new[] { BuildTrack(query) };
                response.Tracks = track;
                response.LoadType = LoadType.TrackLoaded;
            }

            return new ValueTask<SearchResult>(response);
        }

        public override ValueTask<Stream> GetStreamAsync(string id)
        {
            throw new System.NotImplementedException();
        }

        private AudioTrack BuildTrack(string filePath)
        {
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
        }
    }
}