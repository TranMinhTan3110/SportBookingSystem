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
        public string Message { get; set; } 
    }
    public class UserBookingDTO
    {
      
        public string BookingCode { get; set; }

        
        public string PitchName { get; set; }

        // Ngày đặt sân
        public DateTime BookingDate { get; set; }

     
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

    
        public string TimeRange { get; set; }

    
        public decimal? TotalPrice { get; set; }

      
        public int Status { get; set; }

        public string? CheckInCode { get; set; }
    }
}