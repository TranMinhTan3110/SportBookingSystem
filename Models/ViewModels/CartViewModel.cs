using SportBookingSystem.Models.Entities;

namespace SportBookingSystem.Models.ViewModels
{
    /// <summary>
    /// ViewModel cho trang giỏ hàng
    /// </summary>
    public class CartViewModel
    {
        public List<CartItem> Items { get; set; } = new();
        public decimal TotalAmount => Items.Sum(i => i.Total);
        public decimal WalletBalance { get; set; }
        public bool IsLoggedIn { get; set; }

        /// <summary>Số dư có đủ thanh toán không?</summary>
        public bool HasEnoughBalance => WalletBalance >= TotalAmount;

        /// <summary>Thiếu bao nhiêu?</summary>
        public decimal ShortageAmount => TotalAmount > WalletBalance ? TotalAmount - WalletBalance : 0;

        /// <summary>Tất cả items đều còn hàng?</summary>
        public bool AllItemsAvailable => Items.All(i => i.IsAvailable);

        /// <summary>Thời gian tạo giỏ hàng (null nếu giỏ rỗng)</summary>
        public DateTime? CartCreatedAt { get; set; }

        /// <summary>Số giây còn lại trước khi giỏ hết hạn (mặc định 30 phút)</summary>
        public double RemainingSeconds { get; set; }

        /// <summary>Thời gian timeout (phút)</summary>
        public int CartTimeoutMinutes { get; set; } = 30;

        /// <summary>Giỏ hàng đã hết hạn?</summary>
        public bool IsCartExpired => RemainingSeconds <= 0 && Items.Any();
    }
}
