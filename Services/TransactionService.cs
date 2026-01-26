using Microsoft.EntityFrameworkCore;
using SportBookingSystem.DTO;
using SportBookingSystem.Models.EF;
using SportBookingSystem.Models.Entities;
using SportBookingSystem.Constants;

namespace SportBookingSystem.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly ApplicationDbContext _context;
        public TransactionService(ApplicationDbContext context) => _context = context;

        public async Task<PaymentDashboardDTO> GetPaymentDashboardAsync(int page, int pageSize)
        {
            var query = _context.Transactions
                .Include(t => t.Sender)
                .OrderByDescending(t => t.TransactionDate);

            // Tổng số bản ghi
            var totalRecords = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

            // Lấy dữ liệu phân trang
            var transactions = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Tính toán statistics từ TẤT CẢ giao dịch (không phân trang)
            var allTransactions = await _context.Transactions.ToListAsync();

            return new PaymentDashboardDTO
            {
                Payments = transactions.Select(t => new TransactionItemDTO
                {
                    Code = t.TransactionCode,
                    User = t.Sender?.FullName ?? t.Sender?.Username,
                    Amount = t.Amount,
                    Date = t.TransactionDate,
                    Type = t.TransactionType,
                    Status = t.Status,
                    Source = t.Source
                }).ToList(),
                TotalDeposits = allTransactions
                    .Where(t => t.TransactionType == TransactionTypes.Recharge
                             && t.Status == TransactionStatus.Success)
                    .Sum(t => t.Amount),
                Revenue = allTransactions
                    .Where(t => (t.TransactionType == TransactionTypes.Booking
                              || t.TransactionType == TransactionTypes.Order)
                             && t.Status == TransactionStatus.Success)
                    .Sum(t => t.Amount),
                TransactionCount = totalRecords,
                CurrentPage = page,
                PageSize = pageSize,
                TotalPages = totalPages
            };
        }
        public async Task<UserDepositInfoDTO?> GetUserByPhoneAsync(string phone)
        {
            if (string.IsNullOrEmpty(phone)) return null;
            var cleanPhone = phone.Trim();

            return await _context.Users
                .Where(u => u.Phone == cleanPhone)
                .Select(u => new UserDepositInfoDTO
                {
                    UserId = u.UserId,
                    FullName = u.FullName ?? u.Username,
                    Phone = u.Phone,
                    WalletBalance = u.WalletBalance
                })
                .FirstOrDefaultAsync();
        }

        public async Task<UserDepositInfoDTO?> GetUserByIdAsync(int id)
        {
            return await _context.Users
                .Where(u => u.UserId == id)
                .Select(u => new UserDepositInfoDTO
                {
                    UserId = u.UserId,
                    FullName = u.FullName ?? u.Username,
                    Phone = u.Phone,
                    WalletBalance = u.WalletBalance
                })
                .FirstOrDefaultAsync();
        }

        public async Task<(bool Success, string Message, string? TransactionCode)> CreateDepositAsync(CreateDepositDTO dto)
        {
            using var dbTransaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var user = await _context.Users.FindAsync(dto.UserID);
                if (user == null) return (false, "Khách hàng không tồn tại", null);

                user.WalletBalance += dto.Amount;

                // Xác định prefix dựa trên phương thức thanh toán
                string prefix = GetTransactionPrefix(dto.PaymentMethod);

                var trans = new Transactions
                {
                    UserId = user.UserId,
                    Amount = dto.Amount,
                    TransactionType = TransactionTypes.Recharge,
                    Source = dto.PaymentMethod,
                    Status = TransactionStatus.Success,
                    TransactionDate = DateTime.Now,
                    BalanceAfter = user.WalletBalance,
                    Message = dto.Message
                };

                _context.Transactions.Add(trans);
                await _context.SaveChangesAsync();

                // Tạo mã giao dịch theo prefix
                trans.TransactionCode = $"{prefix}-{trans.TransactionId:D4}";

                await _context.SaveChangesAsync();
                await dbTransaction.CommitAsync();
                return (true, "Nạp tiền thành công", trans.TransactionCode);
            }
            catch (Exception ex)
            {
                await dbTransaction.RollbackAsync();
                return (false, ex.Message, null);
            }
        }

        // Helper method để xác định prefix
        private string GetTransactionPrefix(string paymentMethod)
        {
            return paymentMethod switch
            {
                "Tiền mặt" or "Cash" => TransactionPrefixes.Recharge,
                "Chuyển khoản" or "Banking" => TransactionPrefixes.Deposit,
                "Momo" => TransactionPrefixes.Deposit,
                "VNPay" => TransactionPrefixes.Deposit,
                _ => TransactionPrefixes.Recharge
            };
        }
        public async Task<PaymentDashboardDTO> GetFilteredPaymentsAsync(
           int page,
           int pageSize,
           string? searchTerm = null,
           string? type = null,
           string? status = null,
           DateTime? date = null)
        {
            var query = _context.Transactions
                .Include(t => t.Sender)
                .AsQueryable();

            // Apply search filter (by transaction code or user name)
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.Trim().ToLower();
                query = query.Where(t =>
                    t.TransactionCode.ToLower().Contains(searchTerm) ||
                    (t.Sender != null && (
                        t.Sender.FullName.ToLower().Contains(searchTerm) ||
                        t.Sender.Username.ToLower().Contains(searchTerm)
                    ))
                );
            }

            // Apply type filter
            if (!string.IsNullOrWhiteSpace(type) && type != "all")
            {
                // Handle grouped types
                if (type == "Thanh toán")
                {
                    query = query.Where(t =>
                        t.TransactionType == TransactionTypes.Booking ||
                        t.TransactionType == TransactionTypes.Order
                    );
                }
                else
                {
                    query = query.Where(t => t.TransactionType == type);
                }
            }

            // Apply status filter
            if (!string.IsNullOrWhiteSpace(status) && status != "all")
            {
                // Map English status to Vietnamese
                var mappedStatus = status switch
                {
                    "Completed" => TransactionStatus.Success,
                    "Pending" => TransactionStatus.Pending,
                    "Cancelled" => TransactionStatus.Canceled,
                    _ => status
                };
                query = query.Where(t => t.Status == mappedStatus);
            }

            // Apply date filter
            if (date.HasValue)
            {
                var filterDate = date.Value.Date;
                query = query.Where(t => t.TransactionDate.Date == filterDate);
            }

            // Order by date descending
            query = query.OrderByDescending(t => t.TransactionDate);

            // Calculate totals from ALL transactions (not filtered)
            var allTransactions = await _context.Transactions.ToListAsync();

            // Calculate filtered count
            var totalRecords = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

            // Apply pagination
            var transactions = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PaymentDashboardDTO
            {
                Payments = transactions.Select(t => new TransactionItemDTO
                {
                    Code = t.TransactionCode,
                    User = t.Sender?.FullName ?? t.Sender?.Username,
                    Amount = t.Amount,
                    Date = t.TransactionDate,
                    Type = t.TransactionType,
                    Status = t.Status,
                    Source = t.Source
                }).ToList(),
                TotalDeposits = allTransactions
                    .Where(t => t.TransactionType == TransactionTypes.Recharge
                             && t.Status == TransactionStatus.Success)
                    .Sum(t => t.Amount),
                Revenue = allTransactions
                    .Where(t => (t.TransactionType == TransactionTypes.Booking
                              || t.TransactionType == TransactionTypes.Order)
                             && t.Status == TransactionStatus.Success)
                    .Sum(t => t.Amount),
                TransactionCount = totalRecords, // Filtered count
                CurrentPage = page,
                PageSize = pageSize,
                TotalPages = totalPages
            };
        }

        public async Task<List<ProductListDTO>> GetProductsAsync()
        {
            return await _context.Products
                .Where(p => p.StockQuantity > 0)
                .Select(p => new ProductListDTO
                {
                    ProductId = p.ProductId,
                    ProductName = p.ProductName,
                    Price = p.Price,
                    Stock = p.StockQuantity
                })
                .ToListAsync();
        }
        public async Task<RewardSettingDTO> GetCurrentRewardSettingsAsync()
        {
            var settings = await _context.SystemSetting.ToListAsync();
            return new RewardSettingDTO
            {
                AmountStep = decimal.TryParse(settings.FirstOrDefault(s => s.SettingKey == "RewardAmountStep")?.SettingValue, out var a) ? a : 10000,
                PointBonus = int.TryParse(settings.FirstOrDefault(s => s.SettingKey == "RewardPointBonus")?.SettingValue, out var p) ? p : 1,
                IsActive = bool.TryParse(settings.FirstOrDefault(s => s.SettingKey == "IsRewardActive")?.SettingValue, out var i) ? i : true
            };
        }
        public async Task SaveRewardSettingsAsync(RewardSettingDTO dto)
        {
            // Danh sách các key cần lưu
            var settings = new Dictionary<string, string>
            {
                { "RewardAmountStep", dto.AmountStep.ToString() },
                { "RewardPointBonus", dto.PointBonus.ToString() },
                { "IsRewardActive", dto.IsActive.ToString().ToLower() }
            };

            foreach (var item in settings)
            {
                var setting = await _context.SystemSetting.FindAsync(item.Key);
                if (setting != null)
                {
                    setting.SettingValue = item.Value;
                }
                else
                {
                    _context.SystemSetting.Add(new SystemSetting
                    {
                        SettingKey = item.Key,
                        SettingValue = item.Value
                    });
                }
            }
            await _context.SaveChangesAsync();
        }

        public async Task<OrderFulfillmentDTO?> GetOrderDetailsByIdAsync(int orderId)
        {
            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);

            if (order == null) return null;

            var firstDetail = order.OrderDetails.FirstOrDefault();

            return new OrderFulfillmentDTO
            {
                OrderId = order.OrderId,
                OrderCode = order.OrderCode,
                CustomerName = order.User?.FullName ?? order.User?.Username,
                ProductName = firstDetail?.Product?.ProductName,
                Quantity = firstDetail?.Quantity ?? 0,
                TotalAmount = order.TotalAmount ?? 0,
                Status = order.Status.ToString(),
                OrderDate = order.OrderDate
            };
        }

        public async Task<(bool Success, string Message)> UpdateOrderStatusAsync(int orderId, string newStatus)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var order = await _context.Orders
                    .Include(o => o.OrderDetails)
                        .ThenInclude(od => od.Product)
                    .FirstOrDefaultAsync(o => o.OrderId == orderId);

                if (order == null) return (false, "Không tìm thấy đơn hàng");

                var dbTransaction = await _context.Transactions.FirstOrDefaultAsync(t => t.OrderId == orderId);
                if (dbTransaction == null) return (false, "Không tìm thấy giao dịch liên quan");

                if (newStatus == TransactionStatus.Success)
                {
                    order.Status = 1;
                    dbTransaction.Status = TransactionStatus.Success;
                }
                else if (newStatus == TransactionStatus.Canceled)
                {
                    if (dbTransaction.Status == TransactionStatus.Success || dbTransaction.Status == TransactionStatus.Canceled)
                    {
                        return (false, "Đơn hàng đã được xử lý trước đó");
                    }

                    order.Status = -1; // Đã hủy
                    dbTransaction.Status = TransactionStatus.Canceled;

                    // Hoàn tiền
                    var user = await _context.Users.FindAsync(order.UserId);
                    if (user != null)
                    {
                        user.WalletBalance += order.TotalAmount ?? 0;
                        
                        // Tạo giao dịch hoàn tiền mới
                        var refundTrans = new Transactions
                        {
                            UserId = user.UserId,
                            Amount = order.TotalAmount ?? 0,
                            TransactionType = TransactionTypes.Refund,
                            Source = "System",
                            Status = TransactionStatus.Success,
                            TransactionDate = DateTime.Now,
                            BalanceAfter = user.WalletBalance,
                            Message = $"Hoàn tiền đơn hàng {order.OrderCode}",
                            OrderId = order.OrderId
                        };
                        _context.Transactions.Add(refundTrans);
                        await _context.SaveChangesAsync(); // Save to get ID

                        refundTrans.TransactionCode = $"{TransactionPrefixes.Refund}-{refundTrans.TransactionId:D4}";
                    }

                    // Trả lại kho
                    foreach (var detail in order.OrderDetails)
                    {
                        var product = await _context.Products.FindAsync(detail.ProductId);
                        if (product != null)
                        {
                            product.StockQuantity += detail.Quantity;
                        }
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return (true, "Cập nhật thành công");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return (false, ex.Message);
            }
        }
        public async Task CancelExpiredOrdersAsync()
        {
            try
            {
                var expirationTime = DateTime.Now.AddMinutes(-15);
                var expiredOrderIds = await _context.Orders
                    .Where(o => o.Status == 0 && o.OrderDate < expirationTime)
                    .Select(o => o.OrderId)
                    .ToListAsync();

                if (!expiredOrderIds.Any()) return;

                foreach (var orderId in expiredOrderIds)
                {
                    await UpdateOrderStatusAsync(orderId, TransactionStatus.Canceled);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error canceling expired orders: {ex.Message}");
            }
        }
    }
}