using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Newtonsoft.Json;

namespace Azure.Calculator.Functions
{
    public class CalculateFunction : CalculateFunctionBase
    {
        [Function("Calculate")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequestData req,
            FunctionContext executionContext)
        {
            
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var data = JsonConvert.DeserializeObject(requestBody) ?? HandleAndLogException<InvalidOperationException>(executionContext, Messages.NoDataFromRequest);
            int num1 = data?.num1;
            int num2 = data?.num2;
            string operation = data?.operation ?? HandleAndLogException<InvalidOperationException>(executionContext, Messages.InvalidOperation);


            if (!Operations.TryGetValue(operation, out var operationFunc))
            {
                HandleAndLogException<InvalidOperationException>(executionContext, Messages.InvalidOperation);
            }

            var result = operationFunc!(num1, num2, executionContext);
            await ServiceHubContext.Clients.All.SendAsync("ReceiveResult", result);
            return new OkResult();
        }
    }
}