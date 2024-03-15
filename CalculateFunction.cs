using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.SignalR.Management;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Immutable;
using System.Reflection;

namespace Azure.Calculator.Functions
{
    public class CalculateFunction
    {
        private readonly ServiceHubContext _serviceHubContext;
        
        public CalculateFunction()
        {
            var connectionString = BuildConfigAndGetConnectionString();

            _serviceHubContext = new ServiceManagerBuilder()
                .WithOptions(option =>
                {
                    option.ConnectionString = connectionString;
                    option.ServiceTransportType = ServiceTransportType.Transient;
                })
                .BuildServiceManager()
                .CreateHubContextAsync("calculator", CancellationToken.None)
                .GetAwaiter()
                .GetResult();
        }

        private static string BuildConfigAndGetConnectionString()
        {
            var builder = new ConfigurationBuilder();
            builder.AddUserSecrets(Assembly.GetExecutingAssembly());
            var configuration = builder.Build();
            var connectionString = configuration.GetValue<string>("AzureSignalRConnectionString") ?? throw new InvalidOperationException();
            return connectionString;
        }


        public readonly ImmutableDictionary<string, Func<int, int, FunctionContext, int>> Operations =
            new Dictionary<string, Func<int, int, FunctionContext, int>>
            {
                { "add", (a, b, _) => a + b },
                { "subtract", (a, b, _) => a - b },
                { "multiply", (a, b, _) => a * b },
                { "divide", (a, b, context) => b != 0 ? a / b : HandleAndLogException<ArgumentException>(context, Messages.DivideByZero) }
            }.ToImmutableDictionary();


        [Function("Calculate")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequestData req,
            FunctionContext executionContext)
        {
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var data = JsonConvert.DeserializeObject(requestBody) ?? HandleAndLogException<InvalidOperationException>(executionContext, Messages.NoDataFromRequest);
            string operation = data.operation ?? HandleAndLogException<InvalidOperationException>(executionContext, Messages.InvalidOperation);
            int num1 = data.num1;
            int num2 = data.num2;
            var result = Operations[operation](num1,num2, executionContext);

            await _serviceHubContext.Clients.All.SendAsync("ReceiveResult", result);
            return new OkResult();
        }

        public static dynamic HandleAndLogException<TEnt>(FunctionContext executionContext, string message) where TEnt : Exception
        {
            var logger = executionContext.GetLogger("Calculate");
            logger.LogError(message);
            throw (TEnt)Activator.CreateInstance(typeof(TEnt), message);
        }
    }
}