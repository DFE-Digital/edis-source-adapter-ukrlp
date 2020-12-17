using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dfe.Edis.SourceAdapter.Ukrlp.Domain.Configuration;
using Dfe.Edis.SourceAdapter.Ukrlp.Domain.UkrlpApi;
using Dfe.Edis.SourceAdapter.Ukrlp.Infrastructure.UkrlpSoapApi.Models;
using Dfe.Edis.SourceAdapter.Ukrlp.Infrastructure.UkrlpSoapApi.Serialization;
using Microsoft.Extensions.Logging;

namespace Dfe.Edis.SourceAdapter.Ukrlp.Infrastructure.UkrlpSoapApi
{
    public class UkrlpSoapApiClient : IUkrlpApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly UkrlpApiConfiguration _configuration;
        private readonly ILogger<UkrlpSoapApiClient> _logger;
        private readonly IRequestSerializer _requestSerializer;
        private readonly IResponseDeserializer _responseDeserializer;

        internal UkrlpSoapApiClient(
            HttpClient httpClient,
            UkrlpApiConfiguration configuration,
            ILogger<UkrlpSoapApiClient> logger,
            IRequestSerializer requestSerializer,
            IResponseDeserializer responseDeserializer)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri(configuration.WebServiceUrl, UriKind.Absolute);

            _configuration = configuration;
            _logger = logger;
            _requestSerializer = requestSerializer;
            _responseDeserializer = responseDeserializer;
        }

        public UkrlpSoapApiClient(
            HttpClient httpClient,
            UkrlpApiConfiguration configuration,
            ILogger<UkrlpSoapApiClient> logger)
            : this(httpClient, configuration, logger, new RequestSerializer(), new ResponseDeserializer())
        {
        }

        public async Task<Provider[]> GetProvidersChangedSinceAsync(DateTime changedSince, CancellationToken cancellationToken)
        {
            var statusesToQuery = new[]
            {
                ProviderStatusEnum.A,
                ProviderStatusEnum.V,
                ProviderStatusEnum.PD1,
                ProviderStatusEnum.PD2,
            };

            var results = new List<Provider>();
            foreach (var status in statusesToQuery)
            {
                var request = new ProviderQueryRequest
                {
                    SelectionCriteria = new SelectionCriteria
                    {
                        ProviderUpdatedSince = changedSince,
                        CriteriaCondition = CriteriaConditionEnum.OR,
                        ProviderStatus = status,
                        ApprovedProvidersOnly = ApprovedProvidersOnlyEnum.No,
                        StakeholderId = _configuration.StakeholderId,
                    },
                    QueryId = DateTime.Now.Ticks.ToString(),
                };
                var providersOfStatus = await SendRequestAsync(request, cancellationToken);
                var unreadProviders = providersOfStatus
                    .Where(p1 => !results.Any(p2 => p1.UnitedKingdomProviderReferenceNumber == p2.UnitedKingdomProviderReferenceNumber))
                    .ToArray();
                if (unreadProviders.Any())
                {
                    results.AddRange(unreadProviders);
                }
            }

            return results.ToArray();
        }

        private async Task<Provider[]> SendRequestAsync(ProviderQueryRequest request, CancellationToken cancellationToken)
        {
            var requestXml = _requestSerializer.Serialize(request);
            var httpRequest = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                Headers =
                {
                    {"SOAPAction", "retrieveAllProviders"}
                },
                Content = new StringContent(requestXml, Encoding.UTF8, "text/xml"),
            };

            var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
            var responseXml = await response.Content.ReadAsStringAsync();
            var result = _responseDeserializer.DeserializeResponse(responseXml);

            if (!response.IsSuccessStatusCode)
            {
                // If we are here then we got a failed response without a SOAP fault
                throw new Exception($"Error calling UKRLP SOAP Api. Not fault returned. Http Status {(int) response.StatusCode}");
            }

            return result;
        }
    }
}