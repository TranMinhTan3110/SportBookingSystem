namespace SportBookingSystem.DTO
{
    public class UserTransferDTO
    {
        public string TransactionCode { get; set; }
        public decimal Amount { get; set; }
        public bool IsSender { get; set; } // true = gửi, false = nhận
        public string AmountDisplay => (IsSender ? "-" : "+") + Amount.ToString("N0") + "₫";
        public string AmountClass => IsSender ? "negative" : "positive";
        public string SenderName { get; set; }
        public string ReceiverName { get; set; }
        public DateTime Date { get; set; }
        public string Status { get; set; }
        public string Message { get; set; }
    }
}