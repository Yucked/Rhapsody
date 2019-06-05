using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Threading;
using System.Threading.Tasks;
using Frostbyte.Attributes;
using Frostbyte.Entities;
using Frostbyte.Entities.Audio;
using Frostbyte.Entities.Enums;
using Frostbyte.Handlers;
using Microsoft.Extensions.DependencyInjection;

namespace Frostbyte.Sources
{
    [Service(ServiceLifetime.Singleton, typeof(ISourceProvider))]
    public sealed class LocalSource : ISearchProvider, IStreamProvider
    {
        public bool IsEnabled { get; }

        public string Prefix => "lclsearch";

        public LocalSource(ConfigEntity config)
        {
            IsEnabled = config.Sources.EnableLocal;
        }

        public async ValueTask<RESTEntity> SearchAsync(string query, CancellationToken token = default)
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

            return response;
        }

        public ValueTask<Stream> GetStreamAsync(IAudioItem audioItem, CancellationToken token = default)
        {
            throw new System.NotImplementedException();
        }

        public ValueTask<Stream> GetStreamAsync(string id, CancellationToken token = default)
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