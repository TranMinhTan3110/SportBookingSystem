using System;

namespace SportBookingSystem.Models.DTOs
{
    public class PaymentDTO
    {
        public string? Code { get; set; } 
        public string? User { get; set; }
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public string? Type { get; set; }    // "Nạp tiền", "Thanh toán Booking", "Thanh toán Order"
        public string? Status { get; set; }  // "Completed", "Pending", "Cancelled"
        public string? Source { get; set; }  // "Momo", "Banking", "Ví nội bộ"
    }
}
