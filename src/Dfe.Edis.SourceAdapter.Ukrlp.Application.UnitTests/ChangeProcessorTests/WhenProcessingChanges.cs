using System;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using Dfe.Edis.SourceAdapter.Ukrlp.Domain.DataServicesPlatform;
using Dfe.Edis.SourceAdapter.Ukrlp.Domain.StateManagement;
using Dfe.Edis.SourceAdapter.Ukrlp.Domain.UkrlpApi;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace Dfe.Edis.SourceAdapter.Ukrlp.Application.UnitTests.ChangeProcessorTests
{
    public class WhenProcessingChanges
    {
        private Mock<IUkrlpApiClient> _ukrlpApiClientMock;
        private Mock<IStateStore> _stateStoreMock;
        private Mock<IUkrlpDataReceiver> _ukrlpDataReceiverMock;
        private Mock<ILogger<ChangeProcessor>> _loggerMock;
        private ChangeProcessor _changeProcessor;
        private CancellationToken _cancellationToken;

        [SetUp]
        public void Arrange()
        {
            _ukrlpApiClientMock = new Mock<IUkrlpApiClient>();

            _stateStoreMock = new Mock<IStateStore>();
            _stateStoreMock.Setup(store => store.GetStateAsync("LastChecked", It.IsAny<CancellationToken>()))
                .ReturnsAsync("2020-12-17T14:39:00Z");

            _ukrlpDataReceiverMock = new Mock<IUkrlpDataReceiver>();

            _loggerMock = new Mock<ILogger<ChangeProcessor>>();

            _changeProcessor = new ChangeProcessor(
                _ukrlpApiClientMock.Object,
                _stateStoreMock.Object,
                _ukrlpDataReceiverMock.Object,
                _loggerMock.Object);

            _cancellationToken = new CancellationToken();
        }

        [Test]
        public async Task ThenItShouldGetLastTimeUkrlpWasCheckedForChange()
        {
            await _changeProcessor.ProcessChangesAsync(_cancellationToken);

            _stateStoreMock.Verify(store => store.GetStateAsync("LastChecked", _cancellationToken),
                Times.Once);
        }

        [Test, AutoData]
        public async Task ThenItShouldGetProvidersChangedSinceLastCheckedFromUkrlp(DateTime lastChecked)
        {
            _stateStoreMock.Setup(store => store.GetStateAsync("LastChecked", It.IsAny<CancellationToken>()))
                .ReturnsAsync(lastChecked.ToString("O"));

            await _changeProcessor.ProcessChangesAsync(_cancellationToken);

            _ukrlpApiClientMock.Verify(client =>
                    client.GetProvidersChangedSinceAsync(It.Is<DateTime>(arg => Math.Abs((arg - lastChecked).TotalSeconds) < 1), _cancellationToken),
                Times.Once);
        }

        [Test]
        public async Task ThenItShouldGetProvidersChangedSinceTheBeginningOfTheDayFromUkrlpIfNEverCheckedBefore()
        {
            _stateStoreMock.Setup(store => store.GetStateAsync("LastChecked", It.IsAny<CancellationToken>()))
                .ReturnsAsync((string) null);

            await _changeProcessor.ProcessChangesAsync(_cancellationToken);

            _ukrlpApiClientMock.Verify(client =>
                    client.GetProvidersChangedSinceAsync(It.Is<DateTime>(arg => Math.Abs((arg - DateTime.Today).TotalSeconds) < 1), _cancellationToken),
                Times.Once);
        }

        [Test, AutoData]
        public async Task ThenItShouldSendEachProviderToDataServicePlatform(Provider provider1, Provider provider2)
        {
            _ukrlpApiClientMock.Setup(client => client.GetProvidersChangedSinceAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new[] {provider1, provider2});

            await _changeProcessor.ProcessChangesAsync(_cancellationToken);

            _ukrlpDataReceiverMock.Verify(receiver => receiver.SendDataAsync(It.IsAny<Provider>(), It.IsAny<CancellationToken>()),
                Times.Exactly(2));
            _ukrlpDataReceiverMock.Verify(receiver => receiver.SendDataAsync(provider1, _cancellationToken),
                Times.Once);
            _ukrlpDataReceiverMock.Verify(receiver => receiver.SendDataAsync(provider2, _cancellationToken),
                Times.Once);
        }

        [Test]
        public async Task ThenItShouldSetLastCheckedToNow()
        {
            await _changeProcessor.ProcessChangesAsync(_cancellationToken);

            _stateStoreMock.Verify(
                store => store.SetStateAsync("LastChecked", 
                    It.Is<string>(value => Math.Abs((DateTime.Parse(value) - DateTime.Now).TotalSeconds) < 1), _cancellationToken),
                Times.Once);
        }
    }
}