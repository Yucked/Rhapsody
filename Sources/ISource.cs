using Frostbyte.Entities;
using System.IO;
using System.Threading.Tasks;

namespace Frostbyte.Sources
{
    public interface ISource
    {
        string Prefix { get; }

        bool IsEnabled { get; }

        ValueTask<RESTEntity> PrepareResponseAsync(string query);

        ValueTask<Stream> GetStreamAsync(string id);

        ValueTask<Stream> GetStreamAsync(TrackEntity track);
    }
}