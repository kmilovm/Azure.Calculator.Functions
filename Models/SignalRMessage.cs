namespace Azure.Calculator.Functions.Models
{
    public class SignalRMsg
    {
        public string UserId { get; set; }
        public decimal Num1 { get; set; }
        public decimal Num2 { get; set; }
        public string Operation { get; set; }
    }
}
