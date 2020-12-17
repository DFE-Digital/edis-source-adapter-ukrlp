using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dfe.Edis.SourceAdapter.Ukrlp.Domain.Configuration;
using Dfe.Edis.SourceAdapter.Ukrlp.Domain.DataServicesPlatform;
using Dfe.Edis.SourceAdapter.Ukrlp.Domain.UkrlpApi;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Dfe.Edis.SourceAdapter.Ukrlp.Infrastructure.Kafka.RestProxy
{
    public class KafkaRestProxyUkrlpDataReceiver : IUkrlpDataReceiver
    {
        private readonly HttpClient _httpClient;
        private readonly DataServicePlatformConfiguration _configuration;
        private readonly ILogger<KafkaRestProxyUkrlpDataReceiver> _logger;

        public KafkaRestProxyUkrlpDataReceiver(
            HttpClient httpClient,
            DataServicePlatformConfiguration configuration,
            ILogger<KafkaRestProxyUkrlpDataReceiver> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _httpClient.BaseAddress = new Uri(
                new Uri(configuration.KafkaRestProxyUrl, UriKind.Absolute),
                new Uri($"topics/{configuration.UkrlpProviderTopic}", UriKind.Relative));
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.kafka.v2+json"));
            
            _logger = logger;
        }
        
        public async Task SendDataAsync(Provider provider, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Sending {UKPRN} to Kafka topic {TopicName}",
                provider.UnitedKingdomProviderReferenceNumber, _configuration.UkrlpProviderTopic);
            
            var message = new RestProxyPublishMessage<long, Provider>
            {
                Records = new[]
                {
                    new RestProxyPublishMessageRecord<long, Provider>
                    {
                        Key = provider.UnitedKingdomProviderReferenceNumber,
                        Value = provider,
                    },
                },
            };

            var messageJson = JsonConvert.SerializeObject(message);
            var content = new StringContent(messageJson, Encoding.UTF8, "application/vnd.kafka.json.v2+json");
            var response = await _httpClient.PostAsync("", content, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                throw await RestProxyException.FromFailedHttpResponseAsync($"posting message to {_configuration.UkrlpProviderTopic}", response);
            }

            var responseJson = await response.Content.ReadAsStringAsync();
            var responseMessage = JsonConvert.DeserializeObject<RestProxyPublishMessageResponse>(responseJson);
            if (!string.IsNullOrEmpty(responseMessage.Offsets[0].Error) || !string.IsNullOrEmpty(responseMessage.Offsets[0].ErrorCode))
            {
                throw RestProxyException.FromErroredOffset(responseMessage.Offsets[0], response.StatusCode);
            }
            
            _logger.LogInformation("Message for {UKPRN} stored as offset {Offset} in partition {Partition} for {TopicName}",
                provider.UnitedKingdomProviderReferenceNumber, responseMessage.Offsets[0].Offset, responseMessage.Offsets[0].Partition, _configuration.UkrlpProviderTopic);
        }
    }
}