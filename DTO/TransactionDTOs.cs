namespace SportBookingSystem.DTO
{
    // DTO để nạp tiền
    public class CreateDepositDTO
    {
        public int UserID { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string? Message { get; set; }
    }

    // DTO trả về khi tìm khách hàng bằng SĐT
    public class UserDepositInfoDTO
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public decimal WalletBalance { get; set; }
    }

    // ViewModel cho toàn bộ Dashboard
    public class PaymentDashboardDTO
    {
        public List<TransactionItemDTO> Payments { get; set; } = new List<TransactionItemDTO>();
        public decimal TotalDeposits { get; set; }
        public decimal Revenue { get; set; }
        public int TransactionCount { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

    public class TransactionItemDTO
    {
        public string? Code { get; set; }
        public string? User { get; set; }
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public string? Type { get; set; }
        public string? Status { get; set; }
        public string? Source { get; set; }
    }

    // DTO cho danh sách sản phẩm
    public class ProductListDTO
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Stock { get; set; }
    }

    // DTO cho chi tiết giao dịch
    public class TransactionDetailDTO
    {
        public string? Code { get; set; }
        public string? User { get; set; }
        public string? Phone { get; set; }
        public decimal Amount { get; set; }
        public decimal? BalanceAfter { get; set; }
        public DateTime Date { get; set; }
        public string? Type { get; set; }
        public string? Status { get; set; }
        public string? Source { get; set; }
        public string? Message { get; set; }

        public List<OrderItemDetailDTO> Items { get; set; } = new();
        public BookingInfoDetailDTO? Booking { get; set; }
    }

    public class OrderItemDetailDTO
    {
        public string? ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal Total => Quantity * Price;
    }

    public class BookingInfoDetailDTO
    {
        public string? PitchName { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public decimal TotalPrice { get; set; }
    }
}