using System.Threading;
using System.Threading.Tasks;
using Frostbyte.Entities;

namespace Frostbyte.Sources
{
    public interface ISearchProvider : ISourceProvider
    {
        ValueTask<RESTEntity> SearchAsync(
            string query,
            CancellationToken cancellationToken = default);
    }
}