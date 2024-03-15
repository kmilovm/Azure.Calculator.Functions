using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.SignalR.Management;
using Microsoft.Extensions.Logging;

namespace Azure.Calculator.Functions
{
    public class CalculateFunctionBase
    {
        private static readonly string ConnectionString = Environment.GetEnvironmentVariable("AzureSignalRConnectionString") ?? throw new InvalidOperationException();

        public static readonly Dictionary<string, Func<int, int, FunctionContext, int>> Operations = new()
        {
            { "add", (a, b, context) => a + b },
            { "subtract", (a, b, context) => a - b },
            { "multiply", (a, b, context) => a * b },
            { "divide", (a, b, context) => b != 0 ? a / b : HandleAndLogException<ArgumentException>(context, Messages.DivideByZero) }
        };
        
        public static readonly ServiceHubContext ServiceHubContext = new ServiceManagerBuilder()
            .WithOptions(option =>
            {
                option.ConnectionString = ConnectionString;
                option.ServiceTransportType = ServiceTransportType.Transient;
            })
            .BuildServiceManager()
            .CreateHubContextAsync("calculator", CancellationToken.None)
            .GetAwaiter()
            .GetResult();

        public static dynamic HandleAndLogException<TEnt>(FunctionContext executionContext, string message) where TEnt : Exception
        {
            var logger = executionContext.GetLogger("Calculate");
            logger.LogError(message);
            throw (TEnt)Activator.CreateInstance(typeof(TEnt), message);
        }
    }
}