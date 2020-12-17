using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Dfe.Edis.SourceAdapter.Ukrlp.FunctionApp.UnitTests.StartupTests
{
    public class WhenConfigurationFunctionApp
    {
        [Test]
        public void ThenAllFunctionsShouldBeResolvable()
        {
            var functions = GetFunctions();
            var builder = new TestFunctionHostBuilder();
            var configuration = GetTestConfiguration();

            var startup = new Startup();
            startup.Configure(builder, configuration);
            // Have to register the function so container can attempt to resolve them
            foreach (var function in functions)
            {
                builder.Services.AddScoped(function);
            }

            // For some reason the AddHttpClient extensions not resolving under test. Adding this to work around until can figure it out
            builder.Services.AddScoped<HttpClient>(sp => new HttpClient());

            var provider = builder.Services.BuildServiceProvider();

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


        private IConfigurationRoot GetTestConfiguration()
        {
            return new ConfigurationBuilder()
                .AddInMemoryCollection(new[]
                {
                    new KeyValuePair<string, string>("UkrlpApi:WebServiceUrl", "http://localhost:1234"),
                    new KeyValuePair<string, string>("UkrlpApi:StakeholderId", "987654"),
                }).Build();
        }


        private Type[] GetFunctions()
        {
            var functionTriggerTypes = new[]
            {
                typeof(HttpTriggerAttribute),
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

        private class TestFunctionHostBuilder : IFunctionsHostBuilder
        {
            public TestFunctionHostBuilder()
            {
                Services = new ServiceCollection();
            }

            public IServiceCollection Services { get; }
        }
    }
}