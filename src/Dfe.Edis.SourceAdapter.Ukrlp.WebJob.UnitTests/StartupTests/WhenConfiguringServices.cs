using System;
using System.Linq;
using System.Net.Http;
using Dfe.Edis.SourceAdapter.Ukrlp.Domain.Configuration;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Dfe.Edis.SourceAdapter.Ukrlp.WebJob.UnitTests.StartupTests
{
    public class WhenConfiguringServices
    {
        [Test]
        public void ThenAllFunctionsShouldBeResolvable()
        {
            var functions = GetFunctions();
            var serviceCollection = new ServiceCollection();
            var configuration = GetTestConfiguration();

            var startup = new Startup();
            startup.Configure(serviceCollection, configuration);
            // Have to register the function so container can attempt to resolve them
            foreach (var function in functions)
            {
                serviceCollection.AddScoped(function);
            }

            // For some reason the AddHttpClient extensions not resolving under test. Adding this to work around until can figure it out
            serviceCollection.AddScoped<HttpClient>(sp => new HttpClient());

            var provider = serviceCollection.BuildServiceProvider();

            foreach (var function in functions)
            {
                try
                {
                    using (provider.CreateScope())
                    {
                        var resolvedFunction = provider.GetService(function);
                        if (resolvedFunction == null)
                        {
                            throw new NullReferenceException("Function resolved to null");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Assert.Fail($"Failed to resolved {function.Name}:\n{ex}");
                }
            }
        }


        private RootAppConfiguration GetTestConfiguration()
        {
            return new RootAppConfiguration
            {
                UkrlpApi = new UkrlpApiConfiguration
                {
                    WebServiceUrl = "http://localhost:1234",
                    StakeholderId = 987654,
                },
                State = new StateConfiguration
                {
                    BlobConnectionString = "UseDevelopmentStorage=true",
                    BlobContainerName = "unit-tests",
                },
                DataServicePlatform = new DataServicePlatformConfiguration
                {
                    KafkaBootstrapServers = "localhost:1001",
                    KafkaMdsUrl = "http://localhost:1004",
                    KafkaRestProxyUrl = "https://localhost:1003",
                    SchemaRegistryUrl = "https://localhost:1002",
                    UkrlpProviderTopic = "test-topic",
                },
            };
        }


        private Type[] GetFunctions()
        {
            var functionTriggerTypes = new[]
            {
                typeof(TimerTriggerAttribute),
            };

            var allParameters = typeof(Startup).Assembly
                .GetTypes()
                .SelectMany(t => t.GetMethods())
                .SelectMany(m => m.GetParameters());
            var matchingParameters = allParameters
                .Where(p => p.CustomAttributes.Any(a => functionTriggerTypes.Any(t => t == a.AttributeType)))
                .ToArray();
            var functionTypes = matchingParameters
                .Select(m => m.Member.DeclaringType)
                .ToArray();

            return functionTypes;
        }
    }
}