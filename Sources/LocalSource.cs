using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Frostbyte.Attributes;
using Frostbyte.Entities;
using Frostbyte.Entities.Enums;
using Microsoft.Extensions.DependencyInjection;

namespace Frostbyte.Sources
{
    [Service(ServiceLifetime.Singleton, typeof(ISource))]
    public sealed class LocalSource : ISource
    {
        public string Prefix { get; }
        public bool IsEnabled { get; }

        public LocalSource(ConfigEntity config)
        {
            Prefix = "lclsearch";
            IsEnabled = config.Sources.EnableLocal;
        }

        public ValueTask<RESTEntity> PrepareResponseAsync(string query)
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

            return new ValueTask<RESTEntity>(response);
        }

        public async ValueTask<Stream> GetStreamAsync(TrackEntity track)
        {
            throw new System.NotImplementedException();
        }

        public async ValueTask<Stream> GetStreamAsync(string id)
        {
            throw new System.NotImplementedException();
        }

        private TrackEntity BuildTrack(string filePath)
        {
            using var file = TagLib.File.Create(filePath);
            var track = new TrackEntity
            {
                Id = file.Name,
                Title = file.Tag.Title,
                Author = file.Tag.FirstAlbumArtist,
                TrackLength = (int)file.Properties.Duration.TotalMilliseconds
            };

            return track;
        }
    }
}