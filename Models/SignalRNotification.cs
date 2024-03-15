namespace Azure.Calculator.Functions.Models
{
    public class SignalRNotification
    {
        public string UserId { get; set; }
        public string Target { get; set; }
        public string Message { get; set; }
    }
}
