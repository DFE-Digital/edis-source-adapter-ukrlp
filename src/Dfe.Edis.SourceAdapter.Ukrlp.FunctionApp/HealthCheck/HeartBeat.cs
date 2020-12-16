using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;

namespace Dfe.Edis.SourceAdapter.Ukrlp.FunctionApp.HealthCheck
{
    public class HeartBeat
    {
        [FunctionName("HeartBeat")]
        public IActionResult RunAsync(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)]
            HttpRequest req)
        {
            return new OkResult();
        }
    }
}