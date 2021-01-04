using System.Net.Http;
using Dfe.Edis.SourceAdapter.Ukrlp.Application;
using Dfe.Edis.SourceAdapter.Ukrlp.Domain.Configuration;
using Dfe.Edis.SourceAdapter.Ukrlp.Domain.DataServicesPlatform;
using Dfe.Edis.SourceAdapter.Ukrlp.Domain.StateManagement;
using Dfe.Edis.SourceAdapter.Ukrlp.Domain.UkrlpApi;
using Dfe.Edis.SourceAdapter.Ukrlp.Infrastructure.AzureStorage;
using Dfe.Edis.SourceAdapter.Ukrlp.Infrastructure.Kafka.RestProxy;
using Dfe.Edis.SourceAdapter.Ukrlp.Infrastructure.UkrlpSoapApi;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Dfe.Edis.SourceAdapter.Ukrlp.WebJob
{
    public class Startup
    {
        public void Configure(IServiceCollection services, RootAppConfiguration configuration)
        {
            AddConfiguration(services, configuration);

            services.AddHttpClient();

            AddUkrlpApi(services);
            AddUkrlpDataReceiver(services);
            AddState(services);

            services
                .AddScoped<IChangeProcessor, ChangeProcessor>();
        }

        private void AddConfiguration(IServiceCollection services, RootAppConfiguration configuration)
        {
            services.AddSingleton(configuration);
            services.AddSingleton(configuration.UkrlpApi);
            services.AddSingleton(configuration.DataServicePlatform);
            services.AddSingleton(configuration.State);
        }

        private void AddUkrlpApi(IServiceCollection services)
        {
            // Having issues with Typed clients with HTTP extensions. Doing this for now
            services.AddScoped<IUkrlpApiClient, UkrlpSoapApiClient>(sp =>
            {
                var httpClientFactory = sp.GetService<IHttpClientFactory>();
                return new UkrlpSoapApiClient(
                    httpClientFactory.CreateClient(),
                    sp.GetService<UkrlpApiConfiguration>(),
                    sp.GetService<ILogger<UkrlpSoapApiClient>>());
            });
        }

        private void AddUkrlpDataReceiver(IServiceCollection services)
        {
            // Having issues with Typed clients with HTTP extensions. Doing this for now
            services.AddScoped<IUkrlpDataReceiver, KafkaRestProxyUkrlpDataReceiver>(sp =>
            {
                var httpClientFactory = sp.GetService<IHttpClientFactory>();
                return new KafkaRestProxyUkrlpDataReceiver(
                    httpClientFactory.CreateClient(),
                    sp.GetService<DataServicePlatformConfiguration>(),
                    sp.GetService<ILogger<KafkaRestProxyUkrlpDataReceiver>>());
            });
        }

        private void AddState(IServiceCollection services)
        {
            services.AddScoped<IStateStore, BlobStateStore>();
        }
    }
}