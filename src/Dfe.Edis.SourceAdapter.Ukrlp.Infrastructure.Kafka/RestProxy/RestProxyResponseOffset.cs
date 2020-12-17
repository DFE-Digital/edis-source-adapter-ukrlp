using Newtonsoft.Json;

namespace Dfe.Edis.SourceAdapter.Ukrlp.Infrastructure.Kafka.RestProxy
{
    public class RestProxyResponseOffset
    {
        public long Partition { get; set; }
        public long Offset { get; set; }
        public string Error { get; set; }
        [JsonProperty("error_code")]
        public string ErrorCode { get; set; }
    }
}