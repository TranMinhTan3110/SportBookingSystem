using System.Collections.Generic;

namespace SportBookingSystem.Models.DTOs
{
    public class PaymentDashboardDTO
    {
        public List<PaymentDTO> Payments { get; set; } = new List<PaymentDTO>();
        
        public decimal TotalDeposits { get; set; } // Tổng tiền nạp vào (User Fund)
        public decimal Revenue { get; set; }       // Doanh thu thực (Sales)
        public int PendingDeposits { get; set; }   // Yêu cầu nạp chờ duyệt
        public int TransactionCount { get; set; }
        public int ActiveUserCount { get; set; }
    }
}
