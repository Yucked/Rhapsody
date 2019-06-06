using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Frostbyte.Attributes;
using Frostbyte.Entities;
using Frostbyte.Entities.Audio;
using Frostbyte.Entities.Enums;

namespace Frostbyte.Sources
{
    [RegisterService(typeof(ISourceProvider))]
    public sealed class LocalSource : ISourceProvider
    {
        public string Prefix { get; }
        public bool IsEnabled { get; }

        public LocalSource(Configuration config)
        {
            Prefix = "lclsearch";
            IsEnabled = config.Sources.EnableLocal;
        }

        public ValueTask<RESTEntity> SearchAsync(string query)
        {
            var response = new RESTEntity();

            if (Directory.Exists(query))
            {
                var files = Directory.EnumerateFiles(query, @"\.(?:wav|mp3|flac|m4a|ogg|wma|webm)$", SearchOption.AllDirectories).ToArray();
                if (files.Length < 1)
                {
                    return new ValueTask<RESTEntity>(response);
                }

                foreach (var file in files)
                {
                    var track = BuildTrack(file);
                    response.AudioItems.Add(track);
                }

                response.LoadType = LoadType.SearchResult;
            }
            else
            {
                var track = BuildTrack(query);
                response.AudioItems.Add(track);
                response.LoadType = LoadType.TrackLoaded;
            }

            return new ValueTask<RESTEntity>(response);
        }

        public ValueTask<Track> GetTrackAsync(string id)
        {
            throw new System.NotImplementedException();
        }


        public ValueTask<Stream> GetStreamAsync(string id)
        {
            throw new System.NotImplementedException();
        }

        public ValueTask<Stream> GetStreamAsync(Track track)
        {
            throw new System.NotImplementedException();
        }

        private Track BuildTrack(string filePath)
        {
            using var file = TagLib.File.Create(filePath);
            var track = new Track
            {
                Id = file.Name,
                Title = file.Tag.Title,
                Author = new Author(file.Tag.FirstAlbumArtist),
                TrackLength = (int)file.Properties.Duration.TotalMilliseconds
            };

            return track;
        }
    }
}