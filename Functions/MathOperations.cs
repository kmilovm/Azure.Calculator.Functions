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
using Newtonsoft.Json;

namespace Azure.Calculator.Functions.Functions
{
    public class MathOperations
    {
        private readonly ISignalRHelper _signalRHelper;

        public MathOperations(ISignalRHelper signalRHelper)
        {
            _signalRHelper = signalRHelper;
        }

        public readonly ImmutableDictionary<string, Func<decimal, decimal, decimal>> Operations =
            new Dictionary<string, Func<decimal, decimal, decimal>>
            {
                { "add", decimal.Add },
                { "subtract", decimal.Subtract },
                { "multiply", decimal.Multiply },
                { "divide", (a, b) => b != 0 ? decimal.Divide(a,b) : throw new ArgumentException(Messages.DivideByZero) }
            }.ToImmutableDictionary();

        [FunctionName("Calculate")]
        public async Task<IActionResult> Calculate(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req)
        {
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var message = JsonConvert.DeserializeObject<SignalRMsg>(requestBody) ?? throw new InvalidOperationException(Messages.NoDataFromRequest);
            var operation = message.Operation ?? throw new InvalidOperationException(Messages.InvalidOperation);
            var result = Operations[operation](message.Num1, message.Num2);
            await _signalRHelper.SendMessage(message.UserId, new SignalRNotification()
            {
                Message = result.ToString(CultureInfo.InvariantCulture),
                Target = "ReceiveMessage",
                UserId = message.UserId
            });
            return new OkObjectResult(result);
        }
    }
}
