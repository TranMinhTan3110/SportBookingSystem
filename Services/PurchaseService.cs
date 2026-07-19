using Microsoft.EntityFrameworkCore;
using SportBookingSystem.Models.EF;
using SportBookingSystem.Models.Entities;
using SportBookingSystem.Constants;
using System;
using System.Linq;
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
                    var user = await _context.Users.FindAsync(userId);
                    var product = await _context.Products.FindAsync(productId);

                    if (user == null) return (false, "Không tìm thấy người dùng!", null);
                    if (product == null) return (false, "Không tìm thấy sản phẩm!", null);
                    if (product.StockQuantity < quantity) return (false, "Số lượng tồn kho không đủ!", null);

                    decimal totalAmount = product.Price * quantity;

                    if (user.WalletBalance < totalAmount)
                    {
                        return (false, "Số dư tài khoản không đủ để thanh toán!", null);
                    }

                    user.WalletBalance -= totalAmount;
                    product.StockQuantity -= quantity;

                    var order = new Orders
                    {
                        UserId = userId,
                        OrderDate = DateTime.Now,
                        TotalAmount = totalAmount,
                        Status = OrderStatus.Pending, // 0: Chờ xử lý (Pending)
                        OrderCode = $"ORD-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}"
                    };
                    _context.Orders.Add(order);
                    await _context.SaveChangesAsync();

                    var orderDetail = new OrderDetails
                    {
                        OrderId = order.OrderId,
                        ProductId = productId,
                        Quantity = quantity,
                        Price = product.Price
                    };
                    _context.OrderDetails.Add(orderDetail);

                    var trans = new Transactions
                    {
                        UserId = userId,
                        Amount = totalAmount,
                        TransactionDate = DateTime.Now,
                        TransactionType = TransactionTypes.Order,
                        TransactionCode = order.OrderCode,
                        Status = TransactionStatus.Pending,
                        Source = "Ví nội bộ",
                        BalanceAfter = user.WalletBalance,
                        Message = $"Mua {quantity} x {product.ProductName}",
                        OrderId = order.OrderId
                    };
                    _context.Transactions.Add(trans);

                    await _context.SaveChangesAsync();

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

        public async Task<(bool Success, string Message, string? QrCode)> CheckoutCartAsync(int userId, List<CartItem> cartItems)
        {
            if (!cartItems.Any())
                return (false, "Giỏ hàng trống!", null);

            using (var dbTransaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var user = await _context.Users.FindAsync(userId);
                    if (user == null) return (false, "Không tìm thấy người dùng!", null);

                    // Validate all products + tính tổng tiền
                    decimal totalAmount = 0;
                    var validItems = new List<(CartItem CartItem, Products Product)>();

                    foreach (var item in cartItems)
                    {
                        var product = await _context.Products.FindAsync(item.ProductId);
                        if (product == null)
                            return (false, $"Sản phẩm \"{item.ProductName}\" không tồn tại!", null);
                        if (product.StockQuantity < item.Quantity)
                            return (false, $"Sản phẩm \"{product.ProductName}\" không đủ hàng (còn: {product.StockQuantity})!", null);

                        totalAmount += product.Price * item.Quantity;
                        validItems.Add((item, product));
                    }

                    // Kiểm tra số dư
                    if (user.WalletBalance < totalAmount)
                        return (false, $"Số dư không đủ! Cần {totalAmount:N0}đ, hiện có {user.WalletBalance:N0}đ", null);

                    // Trừ tiền ví
                    user.WalletBalance -= totalAmount;

                    // Giảm tồn kho từng sản phẩm
                    foreach (var (cartItem, product) in validItems)
                    {
                        product.StockQuantity -= cartItem.Quantity;
                    }

                    // Tạo 1 Order
                    var order = new Orders
                    {
                        UserId = userId,
                        OrderDate = DateTime.Now,
                        TotalAmount = totalAmount,
                        Status = OrderStatus.Pending,
                        OrderCode = $"ORD-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}"
                    };
                    _context.Orders.Add(order);
                    await _context.SaveChangesAsync();

                    // Tạo N OrderDetails
                    foreach (var (cartItem, product) in validItems)
                    {
                        var orderDetail = new OrderDetails
                        {
                            OrderId = order.OrderId,
                            ProductId = product.ProductId,
                            Quantity = cartItem.Quantity,
                            Price = product.Price
                        };
                        _context.OrderDetails.Add(orderDetail);
                    }

                    // Tạo 1 Transaction
                    var productNames = string.Join(", ", validItems.Select(v => $"{v.CartItem.Quantity}x {v.Product.ProductName}"));
                    var trans = new Transactions
                    {
                        UserId = userId,
                        Amount = totalAmount,
                        TransactionDate = DateTime.Now,
                        TransactionType = TransactionTypes.Order,
                        TransactionCode = order.OrderCode,
                        Status = TransactionStatus.Pending,
                        Source = "Ví nội bộ",
                        BalanceAfter = user.WalletBalance,
                        Message = $"Mua: {productNames}",
                        OrderId = order.OrderId
                    };
                    _context.Transactions.Add(trans);

                    await _context.SaveChangesAsync();

                    // Tạo QR
                    string qrData = $"CART:{order.OrderId}:{userId}";
                    string qrBase64 = _qrService.GenerateQrCode(qrData);

                    await dbTransaction.CommitAsync();

                    return (true, "Thanh toán giỏ hàng thành công!", qrBase64);
                }
                catch (Exception ex)
                {
                    await dbTransaction.RollbackAsync();
                    return (false, $"Lỗi xử lý: {ex.Message}", null);
                }
            }
        }
    }
}
