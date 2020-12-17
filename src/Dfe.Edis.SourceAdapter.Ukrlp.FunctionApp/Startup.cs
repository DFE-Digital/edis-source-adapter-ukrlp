using System.IO;
using Dfe.Edis.SourceAdapter.Ukrlp.Domain.Configuration;
using Dfe.Edis.SourceAdapter.Ukrlp.Domain.UkrlpApi;
using Dfe.Edis.SourceAdapter.Ukrlp.FunctionApp;
using Dfe.Edis.SourceAdapter.Ukrlp.Infrastructure.UkrlpSoapApi;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

[assembly: FunctionsStartup(typeof(Startup))]
namespace Dfe.Edis.SourceAdapter.Ukrlp.FunctionApp
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            var rawConfiguration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("local.settings.json", true)
                .AddEnvironmentVariables()
                .Build();

            Configure(builder, rawConfiguration);
        }

        public void Configure(IFunctionsHostBuilder builder, IConfigurationRoot configurationRoot)
        {
            var services = builder.Services;

            JsonConvert.DefaultSettings = () =>
                new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver(),
                };

            AddConfiguration(services, configurationRoot);
            AddLogging(services);
            
            AddUkrlpApi(services);
        }
        
        private void AddConfiguration(IServiceCollection services, IConfigurationRoot configurationRoot)
        {
            var configuration = new RootAppConfiguration();
            configurationRoot.Bind(configuration);


            services.AddSingleton(configurationRoot);

            services.AddSingleton(configuration);
        }

        private void AddLogging(IServiceCollection services)
        {
            services.AddLogging(builder => { builder.SetMinimumLevel(LogLevel.Debug); });
        }

        private void AddUkrlpApi(IServiceCollection services)
        {
            services.AddHttpClient<UkrlpSoapApiClient>();
            services.AddScoped<IUkrlpApiClient, UkrlpSoapApiClient>();
        }
    }
}