using Frostbyte.Entities;
using Frostbyte.Entities.Audio;
using System.IO;
using System.Threading.Tasks;

namespace Frostbyte.Sources
{
    public interface ISourceProvider
    {
        string Prefix { get; }

        bool IsEnabled { get; }        

        ValueTask<RESTEntity> SearchAsync(string query);

        ValueTask<Track> GetTrackAsync(string id);

        ValueTask<Stream> GetStreamAsync(string id);

        ValueTask<Stream> GetStreamAsync(Track track);
    }
}