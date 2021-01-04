using System.Threading;
using System.Threading.Tasks;

namespace Dfe.Edis.SourceAdapter.Ukrlp.Domain.StateManagement
{
    public interface IStateStore
    {
        Task<string> GetStateAsync(string key, CancellationToken cancellationToken);
        Task SetStateAsync(string key, string value, CancellationToken cancellationToken);
    }
}