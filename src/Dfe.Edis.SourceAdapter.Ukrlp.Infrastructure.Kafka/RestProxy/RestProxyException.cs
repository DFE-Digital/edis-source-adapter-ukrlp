using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Dfe.Edis.SourceAdapter.Ukrlp.Infrastructure.Kafka.RestProxy
{
    public class RestProxyException : Exception
    {
        public RestProxyException(string message, HttpStatusCode statusCode) : base(message)
        {
            StatusCode = statusCode;
        }

        public HttpStatusCode StatusCode { get; set; }

        public static async Task<RestProxyException> FromFailedHttpResponseAsync(string action, HttpResponseMessage response)
        {
            var message = $"Error {action}, http status {(int) response.StatusCode} returned.";
            if (response.Content != null)
            {
                var content = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrEmpty(content))
                {
                    message += $"{Environment.NewLine}{content}";
                }
            }

            return new RestProxyException(message, response.StatusCode);
        }

        public static RestProxyException FromErroredOffset(RestProxyResponseOffset offset, HttpStatusCode statusCode)
        {
            return new RestProxyException($"Offset reports an error. Partition={offset.Partition}, Offset={offset.Offset}, Code={offset.ErrorCode}" +
                                          $"{Environment.NewLine}{offset.Error}",
                statusCode);
        }
    }
}