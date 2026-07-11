using SportBookingSystem.Models.Entities;

namespace SportBookingSystem.Services
{
    public interface ICartService
    {
        /// <summary>Lấy danh sách items trong giỏ</summary>
        List<CartItem> GetCart();

        /// <summary>Thêm sản phẩm vào giỏ (hoặc cộng dồn số lượng)</summary>
        void AddToCart(CartItem item);

        /// <summary>Cập nhật số lượng (nếu ≤ 0 thì xóa)</summary>
        void UpdateQuantity(int productId, int quantity);

        /// <summary>Xóa 1 sản phẩm khỏi giỏ</summary>
        void RemoveItem(int productId);

        /// <summary>Xóa toàn bộ giỏ hàng</summary>
        void ClearCart();

        /// <summary>Tổng số lượng sản phẩm trong giỏ (dùng cho badge)</summary>
        int GetCartCount();

        /// <summary>Lấy thời điểm tạo giỏ hàng</summary>
        DateTime? GetCartCreationTime();

        /// <summary>Ghi thời điểm tạo giỏ hàng</summary>
        void SetCartCreationTime(DateTime time);

        /// <summary>Tính số giây còn lại trước khi giỏ hết hạn</summary>
        double GetRemainingSeconds();

        /// <summary>Kiểm tra giỏ hàng đã hết hạn chưa</summary>
        bool IsCartExpired();

        /// <summary>Lấy số phút timeout giỏ hàng</summary>
        int GetCartTimeoutMinutes();
    }
}
