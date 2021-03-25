namespace Dfe.Edis.SourceAdapter.Ukrlp.Domain.Configuration
{
    public class DataServicePlatformConfiguration
    {
        public string KafkaBootstrapServers { get; set; }
        public string KafkaMdsUrl { get; set; }
        public string KafkaRestProxyUrl { get; set; }
        public string SchemaRegistryUrl { get; set; }
        public string UkrlpProviderTopic { get; set; }
        
        public string Username { get; set; }
        public string Password { get; set; }
    }
}