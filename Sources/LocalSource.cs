using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Frostbyte.Attributes;
using Frostbyte.Entities;
using Frostbyte.Entities.Audio;
using Frostbyte.Entities.Enums;
using Frostbyte.Handlers;
using Microsoft.Extensions.DependencyInjection;

namespace Frostbyte.Sources
{
    [Service(ServiceLifetime.Singleton, typeof(BaseSource))]
    public sealed class LocalSource : BaseSource
    {
        public override bool IsEnabled
        {
            get => ConfigHandler.Config.Sources.Local;
        }

        public override string Prefix
        {
            get => "lclsearch";
        }

        public override async ValueTask<RESTEntity> PrepareResponseAsync(string query)
        {
            var response = new RESTEntity();

            if (Directory.Exists(query))
            {
                var files = Directory.EnumerateFiles(query, @"\.(?:wav|mp3|flac|m4a|ogg|wma|webm)$", SearchOption.AllDirectories).ToArray();
                if (files.Length < 1)
                {
                    return response;
                }

                foreach (var file in files)
                {
                    var track = BuildTrack(file);
                    response.audioItems.Add(track);
                }

                response.LoadType = LoadType.SearchResult;
            }
            else
            {
                var track = BuildTrack(query);
                response.audioItems.Add(track);
                response.LoadType = LoadType.TrackLoaded;
            }

            return response;
        }

        public override async ValueTask<Stream> GetStreamAsync(Track track)
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
                TrackLength = (int) file.Properties.Duration.TotalMilliseconds
            };

            return track;
        }
    }
}