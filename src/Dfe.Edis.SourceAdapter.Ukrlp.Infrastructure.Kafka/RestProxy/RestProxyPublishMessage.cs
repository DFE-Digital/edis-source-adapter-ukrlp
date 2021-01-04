namespace Dfe.Edis.SourceAdapter.Ukrlp.Infrastructure.Kafka.RestProxy
{
    public class RestProxyPublishMessage<TKey, TValue>
    {
        public RestProxyPublishMessageRecord<TKey, TValue>[] Records { get; set; }
    }
}