using Microsoft.EntityFrameworkCore;
using SportBookingSystem.Models.EF;
using SportBookingSystem.Models.Entities;
using System;
using System.Threading.Tasks;

namespace SportBookingSystem.Services
{
    public class PurchaseService : IPurchaseService
    {
        private readonly ApplicationDbContext _context;
        private readonly IQrService _qrService;

        public PurchaseService(ApplicationDbContext context, IQrService qrService)
        {
            _context = context;
            _qrService = qrService;
        }

        public async Task<(bool Success, string Message, string? QrCode)> PurchaseProductAsync(int userId, int productId, int quantity)
        {
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // 1. Lấy thông tin User và Product
                    var user = await _context.Users.FindAsync(userId);
                    var product = await _context.Products.FindAsync(productId);

                    if (user == null) return (false, "Không tìm thấy người dùng!", null);
                    if (product == null) return (false, "Không tìm thấy sản phẩm!", null);
                    if (product.StockQuantity < quantity) return (false, "Số lượng tồn kho không đủ!", null);

                    decimal totalAmount = product.Price * quantity;

                    // 2. Kiểm tra số dư
                    if (user.WalletBalance < totalAmount)
                    {
                        return (false, "Số dư tài khoản không đủ để thanh toán!", null);
                    }

                    // 3. Trừ tiền và cập nhật kho
                    user.WalletBalance -= totalAmount;
                    product.StockQuantity -= quantity;

                    // 4. Tạo đơn hàng (Order)
                    var order = new Orders
                    {
                        UserId = userId,
                        OrderDate = DateTime.Now,
                        TotalAmount = totalAmount,
                        Status = 1, // Đã thanh toán
                        OrderCode = $"ORD-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}"
                    };
                    _context.Orders.Add(order);
                    await _context.SaveChangesAsync();

                    // 5. Tạo chi tiết đơn hàng (OrderDetail)
                    var orderDetail = new OrderDetails
                    {
                        OrderId = order.OrderId,
                        ProductId = productId,
                        Quantity = quantity,
                        Price = product.Price
                    };
                    _context.OrderDetails.Add(orderDetail);

                    // 6. Tạo giao dịch (Transaction)
                    var trans = new Transactions
                    {
                        UserId = userId,
                        Amount = totalAmount,
                        TransactionDate = DateTime.Now,
                        TransactionType = "Thanh toán mua đồ",
                        TransactionCode = order.OrderCode,
                        Status = "Completed",
                        Source = "Ví nội bộ",
                        BalanceAfter = user.WalletBalance,
                        Message = $"Mua {quantity} x {product.ProductName}",
                        OrderId = order.OrderId
                    };
                    _context.Transactions.Add(trans);

                    await _context.SaveChangesAsync();

                    // 7. Tạo mã QR
                    // Định dạng: PURCHASE:[OrderID]:[UserID]:[ProductID]:[Quantity]
                    string qrData = $"PURCHASE:{order.OrderId}:{userId}:{productId}:{quantity}";
                    string qrBase64 = _qrService.GenerateQrCode(qrData);

                    await transaction.CommitAsync();

                    return (true, "Thanh toán thành công!", qrBase64);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return (false, $"Lỗi xử lý: {ex.Message}", null);
                }
            }
        }
    }
}
