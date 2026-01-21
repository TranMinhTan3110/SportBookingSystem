namespace SportBookingSystem.DTO
{
    public class UserTransactionDTO
    {
        public string TransactionCode { get; set; }
        public decimal Amount { get; set; }
        public bool IsPositive { get; set; }
        public string AmountDisplay => (IsPositive ? "+" : "-") + Amount.ToString("N0") + "đ";
        public string TransactionType { get; set; } // Nạp tiền, Thanh toán sân...
        public string? TransactionSource { get; set; }
        public DateTime Date { get; set; }
        public string Status { get; set; }
        public string Message { get; set; } // Ghi chú: "Admin nạp tiền mặt"
    }
    public class UserBookingDTO
    {
        public string? BookingCode { get; set; }
        public string? PitchName { get; set; }
        public DateTime BookingDate { get; set; }
        //public TimeSpan StartTime { get; set; }
        //public TimeSpan EndTime { get; set; }
        //public decimal TotalAmount { get; set; }
        //public string? Status { get; set; }
    }
}