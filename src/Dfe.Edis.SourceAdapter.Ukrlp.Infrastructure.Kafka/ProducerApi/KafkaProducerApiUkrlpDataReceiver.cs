using System;
using System.Threading;
using System.Threading.Tasks;
using Dfe.Edis.Kafka.Producer;
using Dfe.Edis.SourceAdapter.Ukrlp.Domain.Configuration;
using Dfe.Edis.SourceAdapter.Ukrlp.Domain.DataServicesPlatform;
using Dfe.Edis.SourceAdapter.Ukrlp.Domain.UkrlpApi;
using Microsoft.Extensions.Logging;

namespace Dfe.Edis.SourceAdapter.Ukrlp.Infrastructure.Kafka.ProducerApi
{
    public class KafkaProducerApiUkrlpDataReceiver : IUkrlpDataReceiver
    {
        private readonly IKafkaProducer<string, Provider> _producer;
        private readonly DataServicePlatformConfiguration _configuration;
        private readonly ILogger<KafkaProducerApiUkrlpDataReceiver> _logger;

        public KafkaProducerApiUkrlpDataReceiver(
            IKafkaProducer<string, Provider> producer,
            DataServicePlatformConfiguration configuration,
            ILogger<KafkaProducerApiUkrlpDataReceiver> logger)
        {
            _producer = producer;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendDataAsync(Provider provider, CancellationToken cancellationToken)
        {
            await _producer.ProduceAsync(
                _configuration.UkrlpProviderTopic,
                provider.UnitedKingdomProviderReferenceNumber.ToString(),
                provider,
                cancellationToken);
        }
    }
}