using System.ComponentModel.DataAnnotations;

namespace SportBookingSystem.Models.Entities
{
    /// <summary>
    /// Model cho một sản phẩm trong giỏ hàng (lưu trong Session)
    /// Không phải entity DB — nhưng để trong Entities cho đồng bộ
    /// </summary>
    public class CartItem
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string? CategoryName { get; set; }
        public string? Brand { get; set; }
        public string? Size { get; set; }

        /// <summary>Thành tiền = Price * Quantity</summary>
        public decimal Total => Price * Quantity;

        /// <summary>Tồn kho lúc thêm (để hiển thị cảnh báo)</summary>
        public int StockQuantity { get; set; }

        /// <summary>Còn hàng hay không (load lại từ DB khi vào trang giỏ)</summary>
        public bool IsAvailable { get; set; } = true;
    }
}
