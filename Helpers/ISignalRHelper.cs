using Azure.Calculator.Functions.Models;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using System.Net.Http;
using System.Threading.Tasks;

namespace Azure.Calculator.Functions.Helpers;

public interface ISignalRHelper
{
    Task<SignalRConnectionInfo> RequestConnectionInfo(string userId);

    Task<HttpResponseMessage> SendMessage(string userId, SignalRNotification signalRNotification);
}