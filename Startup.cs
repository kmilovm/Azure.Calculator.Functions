using System;
using System.Reflection;
using Azure.Calculator.Functions;
using Azure.Calculator.Functions.Helpers;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(Startup))]
namespace Azure.Calculator.Functions
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddLogging();
            builder.Services.AddHttpClient();
            builder.Services.AddCors(o => o.AddPolicy("CorsPolicy", policyBuilder =>
            {
                policyBuilder
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials()
                    .SetIsOriginAllowed((host) => true);
            }));
            static ISignalRHelper SignalRHelperFactory(IServiceProvider sp)
            {
                var configuration = sp.GetService<IConfiguration>();
                return new SignalRHelper(configuration);
            }
            builder.Services.AddSingleton(SignalRHelperFactory);

        }

        public override void ConfigureAppConfiguration(IFunctionsConfigurationBuilder builder)
        {
            builder.ConfigurationBuilder.AddEnvironmentVariables().AddUserSecrets(Assembly.GetExecutingAssembly());
            base.ConfigureAppConfiguration(builder);
        }
    }
}
