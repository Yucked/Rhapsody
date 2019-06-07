using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Frostbyte.Attributes;
using Frostbyte.Entities;
using Frostbyte.Entities.Audio;
using Frostbyte.Entities.Enums;
using Frostbyte.Entities.Results;

namespace Frostbyte.Sources
{
    [RegisterService(typeof(ISourceProvider))]
    public sealed class LocalSource : SourceCache, ISourceProvider
    {
        public string Prefix { get; }
        public bool IsEnabled { get; }

        public LocalSource(Configuration config)
        {
            Prefix = "lclsearch";
            IsEnabled = config.Sources.EnableLocal;
        }

        public ValueTask<SearchResult> SearchAsync(string query)
        {
            var response = new SearchResult();

            if (Directory.Exists(query))
            {
                var files = Directory.EnumerateFiles(query, @"\.(?:wav|mp3|flac|m4a|ogg|wma|webm)$", SearchOption.AllDirectories).ToArray();
                if (files.Length < 1)
                {
                    return new ValueTask<SearchResult>(response);
                }

                foreach (var file in files)
                {
                    var track = BuildTrack(file);
                    response.Tracks.Add(track);
                }

                response.LoadType = LoadType.SearchResult;
            }
            else
            {
                var track = BuildTrack(query);
                response.Tracks.Add(track);
                response.LoadType = LoadType.TrackLoaded;
            }

            return new ValueTask<SearchResult>(response);
        }

        public ValueTask<Stream> GetStreamAsync(string id)
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