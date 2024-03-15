using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Azure.Calculator.Functions.Models;

namespace Azure.Calculator.Functions.Helpers
{
    public class SignalRHelper : ISignalRHelper
    {
        private readonly IConfiguration _configuration;

        public SignalRHelper(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<SignalRConnectionInfo> RequestConnectionInfo(string userId)
        {
            var azureNegociateUrl = _configuration["AzureSignalRNegociate"];
            using var client = new HttpClient();
            using var request = new HttpRequestMessage(HttpMethod.Post, azureNegociateUrl);
            if (string.IsNullOrEmpty(userId)) throw new ArgumentNullException(nameof(userId));
            request.Headers.Add("x-ms-client-principal-id", userId);
            var httpResponseMessage = await client.SendAsync(request);
            httpResponseMessage.EnsureSuccessStatusCode();
            if (!httpResponseMessage.IsSuccessStatusCode) return null;
            var result = httpResponseMessage.Content.ReadAsStringAsync().Result;
            return JsonConvert.DeserializeObject<SignalRConnectionInfo>(result);
        }

        public async Task<HttpResponseMessage> SendMessage(string userId, SignalRNotification signalRNotification)
        {
            var requestConnectionInfo = await RequestConnectionInfo(userId);
            if (requestConnectionInfo == default) return new HttpResponseMessage(HttpStatusCode.InternalServerError);
            var azureSendMessageUrl = _configuration["AzureSignalRSendMessage"];
            using var client = new HttpClient();
            using var request = new HttpRequestMessage(HttpMethod.Post, azureSendMessageUrl);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", requestConnectionInfo.AccessToken);
            var signalRNotificationJson = JsonConvert.SerializeObject(signalRNotification);
            request.Content = new StringContent(signalRNotificationJson, Encoding.UTF8, "application/json");
            var httpResponseMessage = await client.SendAsync(request);
            httpResponseMessage.EnsureSuccessStatusCode();
            return httpResponseMessage;
        }
    }
}
