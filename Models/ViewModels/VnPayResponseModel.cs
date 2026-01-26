namespace SportBookingSystem.Models.ViewModels
{
    public class VnPayResponseModel
    {
        public bool Success { get; set; }
        public string TransactionCode { get; set; }
        public decimal Amount { get; set; }
        public string BankCode { get; set; }
        public string BankTransactionNo { get; set; }
        public string CardType { get; set; }
        public string OrderInfo { get; set; }
        public string PayDate { get; set; }
        public string ResponseCode { get; set; }
        public string TransactionNo { get; set; }
        public bool IsValidSignature { get; set; }
    }
}