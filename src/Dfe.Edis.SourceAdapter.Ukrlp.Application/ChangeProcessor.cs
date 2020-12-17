using System;
using System.Threading;
using System.Threading.Tasks;
using Dfe.Edis.SourceAdapter.Ukrlp.Domain.DataServicesPlatform;
using Dfe.Edis.SourceAdapter.Ukrlp.Domain.StateManagement;
using Dfe.Edis.SourceAdapter.Ukrlp.Domain.UkrlpApi;
using Microsoft.Extensions.Logging;

namespace Dfe.Edis.SourceAdapter.Ukrlp.Application
{
    public interface IChangeProcessor
    {
        Task ProcessChangesAsync(CancellationToken cancellationToken);
    }

    public class ChangeProcessor : IChangeProcessor
    {
        private readonly IUkrlpApiClient _ukrlpApiClient;
        private readonly IStateStore _stateStore;
        private readonly IUkrlpDataReceiver _ukrlpDataReceiver;
        private readonly ILogger<ChangeProcessor> _logger;

        public ChangeProcessor(
            IUkrlpApiClient ukrlpApiClient,
            IStateStore stateStore,
            IUkrlpDataReceiver ukrlpDataReceiver,
            ILogger<ChangeProcessor> logger)
        {
            _ukrlpApiClient = ukrlpApiClient;
            _stateStore = stateStore;
            _ukrlpDataReceiver = ukrlpDataReceiver;
            _logger = logger;
        }

        public async Task ProcessChangesAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Getting last date UKRLP was checked");
            var lastCheckedState = await _stateStore.GetStateAsync("LastChecked", cancellationToken);
            DateTime lastChecked;

            if (string.IsNullOrEmpty(lastCheckedState))
            {
                lastChecked = DateTime.Today;
                _logger.LogInformation("Never checked UKRLP for changes before. Starting from {ChangedSinceDate}",
                    lastChecked.ToString("O"));
            }
            else
            {
                lastChecked = DateTime.Parse(lastCheckedState);
                _logger.LogInformation("Getting changes since {ChangedSinceDate}",
                    lastChecked.ToString("O"));
            }

            var changedProviders = await _ukrlpApiClient.GetProvidersChangedSinceAsync(lastChecked, cancellationToken);
            lastChecked = DateTime.Now;

            foreach (var provider in changedProviders)
            {
                await _ukrlpDataReceiver.SendDataAsync(provider, cancellationToken);
            }

            await _stateStore.SetStateAsync("LastChecked", lastChecked.ToString("O"), cancellationToken);
        }
    }
}