using System;
using System.Threading;
using System.Threading.Tasks;

namespace Dfe.Edis.SourceAdapter.Ukrlp.Domain.UkrlpApi
{
    public interface IUkrlpApiClient
    {
        Task<Provider[]> GetProvidersChangedSinceAsync(DateTime changedSince, CancellationToken cancellationToken);
    }
}