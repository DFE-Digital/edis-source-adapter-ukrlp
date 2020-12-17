using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using Dfe.Edis.SourceAdapter.Ukrlp.Domain.Configuration;
using Dfe.Edis.SourceAdapter.Ukrlp.Domain.UkrlpApi;
using Dfe.Edis.SourceAdapter.Ukrlp.Infrastructure.UkrlpSoapApi.Models;
using Dfe.Edis.SourceAdapter.Ukrlp.Infrastructure.UkrlpSoapApi.Serialization;
using Microsoft.Extensions.Logging;
using MockTheWeb;
using Moq;
using NUnit.Framework;
using Times = Moq.Times;

namespace Dfe.Edis.SourceAdapter.Ukrlp.Infrastructure.UkrlpSoapApi.UnitTests.UkrlpSoapApiClientTests
{
    public class WhenGettingProvidersChangedSince
    {
        private HttpClientMock _httpClientMock;
        private UkrlpApiConfiguration _configuration;
        private Mock<ILogger<UkrlpSoapApiClient>> _logger;
        private Mock<IRequestSerializer> _requestSerializer;
        private Mock<IResponseDeserializer> _responseDeserializer;
        private UkrlpSoapApiClient _client;

        [SetUp]
        public void Arrange()
        {
            _httpClientMock = new HttpClientMock();
            _httpClientMock
                .SetDefaultResponse(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("<element />", Encoding.UTF8, "text/xml")
                });

            _configuration = new UkrlpApiConfiguration
            {
                WebServiceUrl = "http://localhost:1234",
                StakeholderId = 123,
            };

            _logger = new Mock<ILogger<UkrlpSoapApiClient>>();

            _requestSerializer = new Mock<IRequestSerializer>();
            _requestSerializer.Setup(serializer => serializer.Serialize(It.IsAny<ProviderQueryRequest>()))
                .Returns("<unit-test>testing</unit-test>");

            _responseDeserializer = new Mock<IResponseDeserializer>();
            _responseDeserializer.Setup(deserializer => deserializer.DeserializeResponse(It.IsAny<string>()))
                .Returns(new Provider[0]);

            _client = new UkrlpSoapApiClient(
                _httpClientMock.AsHttpClient(),
                _configuration,
                _logger.Object,
                _requestSerializer.Object,
                _responseDeserializer.Object);
        }

        [Test, AutoData]
        public async Task ThenItShouldSerializeRequests(DateTime changedSince)
        {
            await _client.GetProvidersChangedSinceAsync(changedSince, CancellationToken.None);

            _requestSerializer.Verify(serializer => serializer.Serialize(It.IsAny<ProviderQueryRequest>()),
                Times.Exactly(4));
            _requestSerializer.Verify(serializer => serializer.Serialize(It.Is<ProviderQueryRequest>(
                    request => !string.IsNullOrEmpty(request.QueryId) &&
                               request.SelectionCriteria != null &&
                               request.SelectionCriteria.ProviderUpdatedSince == changedSince &&
                               request.SelectionCriteria.CriteriaCondition == CriteriaConditionEnum.OR &&
                               request.SelectionCriteria.ProviderStatus == ProviderStatusEnum.A &&
                               request.SelectionCriteria.ApprovedProvidersOnly == ApprovedProvidersOnlyEnum.No &&
                               request.SelectionCriteria.StakeholderId == _configuration.StakeholderId)),
                Times.Once);
            _requestSerializer.Verify(serializer => serializer.Serialize(It.Is<ProviderQueryRequest>(
                    request => !string.IsNullOrEmpty(request.QueryId) &&
                               request.SelectionCriteria != null &&
                               request.SelectionCriteria.ProviderUpdatedSince == changedSince &&
                               request.SelectionCriteria.CriteriaCondition == CriteriaConditionEnum.OR &&
                               request.SelectionCriteria.ProviderStatus == ProviderStatusEnum.V &&
                               request.SelectionCriteria.ApprovedProvidersOnly == ApprovedProvidersOnlyEnum.No &&
                               request.SelectionCriteria.StakeholderId == _configuration.StakeholderId)),
                Times.Once);
            _requestSerializer.Verify(serializer => serializer.Serialize(It.Is<ProviderQueryRequest>(
                    request => !string.IsNullOrEmpty(request.QueryId) &&
                               request.SelectionCriteria != null &&
                               request.SelectionCriteria.ProviderUpdatedSince == changedSince &&
                               request.SelectionCriteria.CriteriaCondition == CriteriaConditionEnum.OR &&
                               request.SelectionCriteria.ProviderStatus == ProviderStatusEnum.PD1 &&
                               request.SelectionCriteria.ApprovedProvidersOnly == ApprovedProvidersOnlyEnum.No &&
                               request.SelectionCriteria.StakeholderId == _configuration.StakeholderId)),
                Times.Once);
            _requestSerializer.Verify(serializer => serializer.Serialize(It.Is<ProviderQueryRequest>(
                    request => !string.IsNullOrEmpty(request.QueryId) &&
                               request.SelectionCriteria != null &&
                               request.SelectionCriteria.ProviderUpdatedSince == changedSince &&
                               request.SelectionCriteria.CriteriaCondition == CriteriaConditionEnum.OR &&
                               request.SelectionCriteria.ProviderStatus == ProviderStatusEnum.PD2 &&
                               request.SelectionCriteria.ApprovedProvidersOnly == ApprovedProvidersOnlyEnum.No &&
                               request.SelectionCriteria.StakeholderId == _configuration.StakeholderId)),
                Times.Once);
        }

        [Test, AutoData]
        public async Task ThenItShouldSendSerializedRequest(
            string serializedARequest,
            string serializedVRequest,
            string serializedPd1Request,
            string serializedPd2Request)
        {
            _requestSerializer.Setup(serializer =>
                    serializer.Serialize(It.Is<ProviderQueryRequest>(req => req.SelectionCriteria.ProviderStatus == ProviderStatusEnum.A)))
                .Returns(serializedARequest);
            _requestSerializer.Setup(serializer =>
                    serializer.Serialize(It.Is<ProviderQueryRequest>(req => req.SelectionCriteria.ProviderStatus == ProviderStatusEnum.V)))
                .Returns(serializedVRequest);
            _requestSerializer.Setup(serializer =>
                    serializer.Serialize(It.Is<ProviderQueryRequest>(req => req.SelectionCriteria.ProviderStatus == ProviderStatusEnum.PD1)))
                .Returns(serializedPd1Request);
            _requestSerializer.Setup(serializer =>
                    serializer.Serialize(It.Is<ProviderQueryRequest>(req => req.SelectionCriteria.ProviderStatus == ProviderStatusEnum.PD2)))
                .Returns(serializedPd2Request);

            await _client.GetProvidersChangedSinceAsync(new DateTime(), CancellationToken.None);

            _httpClientMock.Verify(req => true, MockTheWeb.Times.Exactly(4));
            _httpClientMock.Verify(req => req.Content.ReadAsStringAsync().Result == serializedARequest, MockTheWeb.Times.Once());
            _httpClientMock.Verify(req => req.Content.ReadAsStringAsync().Result == serializedVRequest, MockTheWeb.Times.Once());
            _httpClientMock.Verify(req => req.Content.ReadAsStringAsync().Result == serializedPd1Request, MockTheWeb.Times.Once());
            _httpClientMock.Verify(req => req.Content.ReadAsStringAsync().Result == serializedPd2Request, MockTheWeb.Times.Once());
        }

        [Test, AutoData]
        public async Task ThenItShouldDeserializeResponses(
            string responseA,
            string responseV,
            string responsePd1,
            string responsePd2)
        {
            _requestSerializer
                .Setup(serializer => serializer.Serialize(It.Is<ProviderQueryRequest>(req => req.SelectionCriteria.ProviderStatus == ProviderStatusEnum.A)))
                .Returns("<ProviderStatus>A</ProviderStatus>");
            _httpClientMock
                .When(req => req.Content.ReadAsStringAsync().Result == "<ProviderStatus>A</ProviderStatus>")
                .Then(new HttpResponseMessage {StatusCode = HttpStatusCode.OK, Content = new StringContent(responseA)});
            _requestSerializer
                .Setup(serializer => serializer.Serialize(It.Is<ProviderQueryRequest>(req => req.SelectionCriteria.ProviderStatus == ProviderStatusEnum.V)))
                .Returns("<ProviderStatus>V</ProviderStatus>");
            _httpClientMock
                .When(req => req.Content.ReadAsStringAsync().Result == "<ProviderStatus>V</ProviderStatus>")
                .Then(new HttpResponseMessage {StatusCode = HttpStatusCode.OK, Content = new StringContent(responseV)});
            _requestSerializer
                .Setup(serializer => serializer.Serialize(It.Is<ProviderQueryRequest>(req => req.SelectionCriteria.ProviderStatus == ProviderStatusEnum.PD1)))
                .Returns("<ProviderStatus>PD1</ProviderStatus>");
            _httpClientMock
                .When(req => req.Content.ReadAsStringAsync().Result == "<ProviderStatus>PD1</ProviderStatus>")
                .Then(new HttpResponseMessage {StatusCode = HttpStatusCode.OK, Content = new StringContent(responsePd1)});
            _requestSerializer
                .Setup(serializer => serializer.Serialize(It.Is<ProviderQueryRequest>(req => req.SelectionCriteria.ProviderStatus == ProviderStatusEnum.PD2)))
                .Returns("<ProviderStatus>PD2</ProviderStatus>");
            _httpClientMock
                .When(req => req.Content.ReadAsStringAsync().Result == "<ProviderStatus>PD2</ProviderStatus>")
                .Then(new HttpResponseMessage {StatusCode = HttpStatusCode.OK, Content = new StringContent(responsePd2)});

            await _client.GetProvidersChangedSinceAsync(new DateTime(), CancellationToken.None);

            _responseDeserializer.Verify(deserializer => deserializer.DeserializeResponse(It.IsAny<string>()), Times.Exactly(4));
            _responseDeserializer.Verify(deserializer => deserializer.DeserializeResponse(responseA), Times.Once);
            _responseDeserializer.Verify(deserializer => deserializer.DeserializeResponse(responseV), Times.Once);
            _responseDeserializer.Verify(deserializer => deserializer.DeserializeResponse(responsePd1), Times.Once);
            _responseDeserializer.Verify(deserializer => deserializer.DeserializeResponse(responsePd2), Times.Once);
        }

        [Test, AutoData]
        public async Task ThenItShouldReturnUniqueListOfProvidersFromAllStatuses(long ukprn1, long ukprn2, long ukprn3, long ukprn4, long ukprn5, long ukprn6)
        {
            var count = 0;
            _responseDeserializer.Setup(deserializer => deserializer.DeserializeResponse(It.IsAny<string>()))
                .Returns(() =>
                {
                    count++;
                    if (count == 1)
                    {
                        return new[]
                        {
                            new Provider {UnitedKingdomProviderReferenceNumber = ukprn1},
                            new Provider {UnitedKingdomProviderReferenceNumber = ukprn5},
                        };
                    }

                    if (count == 2)
                    {
                        return new[]
                        {
                            new Provider {UnitedKingdomProviderReferenceNumber = ukprn2},
                            new Provider {UnitedKingdomProviderReferenceNumber = ukprn6},
                        };
                    }

                    if (count == 3)
                    {
                        return new[]
                        {
                            new Provider {UnitedKingdomProviderReferenceNumber = ukprn3},
                            new Provider {UnitedKingdomProviderReferenceNumber = ukprn5},
                        };
                    }

                    return new[]
                    {
                        new Provider {UnitedKingdomProviderReferenceNumber = ukprn4},
                        new Provider {UnitedKingdomProviderReferenceNumber = ukprn6},
                    };
                });

            var actual = await _client.GetProvidersChangedSinceAsync(new DateTime(), CancellationToken.None);

            Assert.IsNotNull(actual);
            Assert.AreEqual(6, actual.Length);
            Assert.AreEqual(1, actual.Count(provider => provider.UnitedKingdomProviderReferenceNumber == ukprn1));
            Assert.AreEqual(1, actual.Count(provider => provider.UnitedKingdomProviderReferenceNumber == ukprn2));
            Assert.AreEqual(1, actual.Count(provider => provider.UnitedKingdomProviderReferenceNumber == ukprn3));
            Assert.AreEqual(1, actual.Count(provider => provider.UnitedKingdomProviderReferenceNumber == ukprn4));
            Assert.AreEqual(1, actual.Count(provider => provider.UnitedKingdomProviderReferenceNumber == ukprn5));
            Assert.AreEqual(1, actual.Count(provider => provider.UnitedKingdomProviderReferenceNumber == ukprn6));
        }
    }
}