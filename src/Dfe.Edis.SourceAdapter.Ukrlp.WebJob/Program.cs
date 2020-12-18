using System.IO;
using System.Threading.Tasks;
using Dfe.Edis.SourceAdapter.Ukrlp.Domain.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Dfe.Edis.SourceAdapter.Ukrlp.WebJob
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var hostBuilder = new HostBuilder();
            
#if DEBUG
            hostBuilder.UseEnvironment("development");
#endif

            var configuration = LoadConfiguration();

            JsonConvert.DefaultSettings = () =>
                new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver(),
                };

            hostBuilder.ConfigureWebJobs((webJobBuilder) =>
            {
                webJobBuilder.AddTimers();
                webJobBuilder.AddAzureStorageCoreServices();
            });
            hostBuilder.ConfigureLogging((context, logBuilder) =>
            {
                logBuilder.SetMinimumLevel(LogLevel.Debug);
                logBuilder.AddConsole();
            });
            hostBuilder.ConfigureServices((context, services) =>
            {
                var startup = new Startup();
                startup.Configure(services, configuration);
            });

            using var host = hostBuilder.Build();
            await host.RunAsync();
        }

        static RootAppConfiguration LoadConfiguration()
        {
            var configurationRoot = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddEnvironmentVariables()
                .Build();
            
            var configuration = new RootAppConfiguration();
            configurationRoot.Bind(configuration);

            return configuration;
        }
    }
}