using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using Dfe.Edis.SourceAdapter.Ukrlp.Domain.Configuration;
using Dfe.Edis.SourceAdapter.Ukrlp.Domain.UkrlpApi;
using Dfe.Edis.SourceAdapter.Ukrlp.Infrastructure.Kafka.RestProxy;
using Microsoft.Extensions.Logging;
using MockTheWeb;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using Times = MockTheWeb.Times;

namespace Dfe.Edis.SourceAdapter.Ukrlp.Infrastructure.Kafka.UnitTests.RestProxyTests
{
    public class WhenSendingDataToRestProxy
    {
        private HttpClientMock _httpClientMock;
        private DataServicePlatformConfiguration _configuration;
        private Mock<ILogger<KafkaRestProxyUkrlpDataReceiver>> _loggerMock;
        private KafkaRestProxyUkrlpDataReceiver _receiver;
        private CancellationToken _cancellationToken;

        [SetUp]
        public void Arrange()
        {
            _httpClientMock = new HttpClientMock();
            _httpClientMock
                .When(c => true)
                .Then(ResponseBuilder.Json(
                    new RestProxyPublishMessageResponse
                    {
                        Offsets = new[]
                        {
                            new RestProxyResponseOffset
                            {
                                Partition = 0,
                                Offset = 1,
                            },
                        },
                    }));

            _configuration = new DataServicePlatformConfiguration
            {
                KafkaRestProxyUrl = "https://localhost:9876",
                UkrlpProviderTopic = "some-topic",
            };

            _loggerMock = new Mock<ILogger<KafkaRestProxyUkrlpDataReceiver>>();

            _receiver = new KafkaRestProxyUkrlpDataReceiver(
                _httpClientMock.AsHttpClient(),
                _configuration,
                _loggerMock.Object);

            _cancellationToken = new CancellationToken();
        }

        [Test, AutoData]
        public async Task ThenItShouldSendProviderToRestProxyTopic(Provider provider)
        {
            await _receiver.SendDataAsync(provider, _cancellationToken);

            var expectedUrl = new Uri(
                new Uri(_configuration.KafkaRestProxyUrl, UriKind.Absolute),
                new Uri($"topics/{_configuration.UkrlpProviderTopic}", UriKind.Relative));
            _httpClientMock.Verify(request => request.RequestUri.AbsoluteUri == expectedUrl.AbsoluteUri,
                Times.Once());

            var expectedContent = JsonConvert.SerializeObject(new RestProxyPublishMessage<long, Provider>
            {
                Records = new[]
                {
                    new RestProxyPublishMessageRecord<long, Provider>
                    {
                        Key = provider.UnitedKingdomProviderReferenceNumber,
                        Value = provider,
                    },
                },
            });
            _httpClientMock.Verify(request => request.Content.ReadAsStringAsync().Result == expectedContent,
                Times.Once());
        }

        [Test]
        public void ThenItShouldThrowAnExceptionIfTheResponseIsNotASuccessCode()
        {
            _httpClientMock
                .When(c => true)
                .Then(ResponseBuilder.Response().WithStatus(HttpStatusCode.InternalServerError));

            var actual = Assert.ThrowsAsync<RestProxyException>(async () =>
                await _receiver.SendDataAsync(new Provider(), _cancellationToken));
            Assert.AreEqual(HttpStatusCode.InternalServerError, actual.StatusCode);
            Assert.AreEqual($"Error posting message to {_configuration.UkrlpProviderTopic}, http status 500 returned.", actual.Message);
        }

        [TestCase("code001", "error details")]
        [TestCase(null, "error details")]
        [TestCase("code001", null)]
        public void ThenItShouldThrowAnExceptionIfTheOffsetContainsError(string errorCode, string error)
        {
            _httpClientMock
                .When(c => true)
                .Then(ResponseBuilder.Json(
                    new RestProxyPublishMessageResponse
                    {
                        Offsets = new[]
                        {
                            new RestProxyResponseOffset
                            {
                                Partition = 0,
                                Offset = 1,
                                Error = error,
                                ErrorCode = errorCode,
                            },
                        },
                    }, new MockTheWebNewtonsoftSerializer()));

            var actual = Assert.ThrowsAsync<RestProxyException>(async () =>
                await _receiver.SendDataAsync(new Provider(), _cancellationToken));
            Assert.AreEqual(HttpStatusCode.OK, actual.StatusCode);
            Assert.AreEqual($"Offset reports an error. Partition=0, Offset=1, Code={errorCode}" +
                            $"{Environment.NewLine}{error}", actual.Message);
        }
    }
}