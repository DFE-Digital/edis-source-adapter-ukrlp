using System.Threading;
using System.Threading.Tasks;
using Dfe.Edis.SourceAdapter.Ukrlp.Domain.UkrlpApi;

namespace Dfe.Edis.SourceAdapter.Ukrlp.Domain.DataServicesPlatform
{
    public interface IUkrlpDataReceiver
    {
        Task SendDataAsync(Provider provider, CancellationToken cancellationToken);
    }
}