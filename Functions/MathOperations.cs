using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Azure.Calculator.Functions.Helpers;
using Azure.Calculator.Functions.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Azure.Calculator.Functions.Functions
{
    public class MathOperations
    {
        private readonly ILoggerFactory _logger;
        private readonly ISignalRHelper _signalRHelper;

        public MathOperations(ILoggerFactory logger, ISignalRHelper signalRHelper)
        {
            _logger = logger;
            _signalRHelper = signalRHelper;
        }

        public readonly ImmutableDictionary<string, Func<decimal, decimal, decimal>> Operations =
            new Dictionary<string, Func<decimal, decimal, decimal>>
            {
                { "add", (a, b) => a + b },
                { "subtract", (a, b) => a - b },
                { "multiply", (a, b) => a * b },
                { "divide", (a, b) => b != 0 ? a / b : throw new ArgumentException(Messages.DivideByZero) }
            }.ToImmutableDictionary();

        [FunctionName("Calculate")]
        public async Task<IActionResult> Calculate(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req)
        {
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var message = JsonConvert.DeserializeObject<SignalRMsg>(requestBody) ?? throw new InvalidOperationException(Messages.NoDataFromRequest);
            string operation = message.Operation ?? HandleAndLogException<InvalidOperationException>(Messages.InvalidOperation);
            var result = Operations[operation](message.Num1, message.Num2);
            await _signalRHelper.SendMessage(message.UserId, new SignalRNotification()
            {
                Message = result.ToString(CultureInfo.InvariantCulture),
                Target = message.UserId,
                UserId = message.UserId
            });
            return new OkObjectResult(result);
        }

        public dynamic HandleAndLogException<TEnt>(string message) where TEnt : Exception
        {
            _logger.CreateLogger<MathOperations>().LogError(message);
            throw (TEnt)Activator.CreateInstance(typeof(TEnt), message);
        }
    }
}
