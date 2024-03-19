using System;
using System.IO;
using System.Threading.Tasks;
using Azure.Calculator.Functions.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Newtonsoft.Json;

namespace Azure.Calculator.Functions.Functions
{
    public class SignalROperations
    {
        [FunctionName("Negotiate")]
        public static SignalRConnectionInfo Negotiate(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req,
            [SignalRConnectionInfo(HubName = "calculator", UserId = "{headers.x-ms-client-principal-id}", ConnectionStringSetting = "AzureSignalRConnectionString")] SignalRConnectionInfo connectionInfo)
        {
            return connectionInfo;
        }

        [FunctionName("SendMessage")]

        public static Task SendMessage(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req,
            [SignalR(HubName = "calculator", ConnectionStringSetting = "AzureSignalRConnectionString")] IAsyncCollector<SignalRMessage> signalRMessages)
        {
            var requestBody =  new StreamReader(req.Body).ReadToEndAsync().Result;
            var data = JsonConvert.DeserializeObject<SignalRNotification>(requestBody) ?? throw new InvalidOperationException(Messages.NoDataFromRequest);
            return signalRMessages.AddAsync(
                new SignalRMessage
                {
                    Target = data.Target,
                    UserId = data.UserId,
                    Arguments = new object[] { data.Message }
                });
        }
    }
}