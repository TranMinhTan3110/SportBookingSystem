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

            var totalRecords = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

            var transactions = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var allTransactions = await _context.Transactions.ToListAsync();

            return new PaymentDashboardDTO
            {
                Payments = transactions.Select(t => new TransactionItemDTO
                {
                    Code = t.TransactionCode,
                    User = t.Sender?.FullName ?? t.Sender?.Username,
                    Amount = Math.Abs(t.Amount), 
                    Date = t.TransactionDate,
                    Type = t.TransactionType,
                    Status = t.Status,
                    Source = t.Source
                }).ToList(),

                TotalDeposits = allTransactions
                    .Where(t => t.TransactionType == TransactionTypes.Recharge && t.Status == TransactionStatus.Success)
                    .Sum(t => t.Amount),

                Revenue = allTransactions
                    .Sum(t =>
                    {
                        if (t.TransactionType == TransactionTypes.Booking || 
                            t.TransactionType == TransactionTypes.Order)
                        {
                            if (t.Status == TransactionStatus.Success || 
                                t.Status == TransactionStatus.Canceled || 
                                t.Status == TransactionStatus.CancelBooking)
                            {
                                return Math.Abs(t.Amount);
                            }
                        }
                        else if ((t.TransactionType == TransactionTypes.Refund || 
                                  t.TransactionType == TransactionTypes.RefundBooking) && 
                                 t.Status == TransactionStatus.Success)
                        {
                            return -Math.Abs(t.Amount);
                        }
                        return 0;
                    }), 

                TransactionCount = totalRecords,
                CurrentPage = page,
                PageSize = pageSize,
                TotalPages = totalPages
            };
        }

        public async Task<PaymentDashboardDTO> GetFilteredPaymentsAsync(
            int page, int pageSize, string? searchTerm = null, string? type = null, string? status = null, DateTime? date = null)
        {
            var query = _context.Transactions
                .Include(t => t.Sender)
                .AsQueryable();

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

            if (!string.IsNullOrWhiteSpace(type) && type != "all")
            {
                if (type == "Thanh toán")
                {
                    query = query.Where(t => t.TransactionType == TransactionTypes.Booking || t.TransactionType == TransactionTypes.Order);
                }
                else if (type == "Hoàn tiền")
                {
                    query = query.Where(t => t.TransactionType == TransactionTypes.Refund || t.TransactionType == TransactionTypes.RefundBooking);
                }
                else
                {
                    query = query.Where(t => t.TransactionType == type);
                }
            }

            if (!string.IsNullOrWhiteSpace(status) && status != "all")
            {
                if (status == "Completed")
                {
                    query = query.Where(t => t.Status == TransactionStatus.Success);
                }
                else if (status == "Pending")
                {
                    query = query.Where(t => t.Status == TransactionStatus.Pending || t.Status == TransactionStatus.PendingConfirm);
                }
                else if (status == "Cancelled")
                {
                    query = query.Where(t => t.Status == TransactionStatus.Canceled || t.Status == TransactionStatus.CancelBooking);
                }
                else
                {
                    query = query.Where(t => t.Status == status);
                }
            }

            if (date.HasValue)
            {
                var filterDate = date.Value.Date;
                query = query.Where(t => t.TransactionDate.Date == filterDate);
            }

            query = query.OrderByDescending(t => t.TransactionDate);

            var allTransactions = await _context.Transactions.ToListAsync();
            var totalRecords = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

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
                    Amount = Math.Abs(t.Amount), 
                    Date = t.TransactionDate,
                    Type = t.TransactionType,
                    Status = t.Status,
                    Source = t.Source
                }).ToList(),

                TotalDeposits = allTransactions
                    .Where(t => t.TransactionType == TransactionTypes.Recharge && t.Status == TransactionStatus.Success)
                    .Sum(t => t.Amount),

                Revenue = allTransactions
                    .Sum(t =>
                    {
                        if (t.TransactionType == TransactionTypes.Booking || 
                            t.TransactionType == TransactionTypes.Order)
                        {
                            if (t.Status == TransactionStatus.Success || 
                                t.Status == TransactionStatus.Canceled || 
                                t.Status == TransactionStatus.CancelBooking)
                            {
                                return Math.Abs(t.Amount);
                            }
                        }
                        else if ((t.TransactionType == TransactionTypes.Refund || 
                                  t.TransactionType == TransactionTypes.RefundBooking) && 
                                 t.Status == TransactionStatus.Success)
                        {
                            return -Math.Abs(t.Amount);
                        }
                        return 0;
                    }),

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
                    _context.SystemSetting.Add(new SystemSetting { SettingKey = item.Key, SettingValue = item.Value });
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
            string statusText = order.Status switch
            {
                1 => "Thành công",
                0 => "Chờ xử lý",
                -1 => "Đã hủy",
                _ => "Không xác định"
            };

            return new OrderFulfillmentDTO
            {
                OrderId = order.OrderId,
                OrderCode = order.OrderCode,
                CustomerName = order.User?.FullName ?? order.User?.Username ?? "Khách vãng lai",
                ProductName = firstDetail?.Product?.ProductName ?? "N/A",
                Quantity = firstDetail?.Quantity ?? 0,
                TotalAmount = order.TotalAmount ?? 0,
                Status = statusText,
                StatusCode = order.Status, 
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

                
                if (order.Status == 1)
                {
                    return (false, "Đơn hàng này đã được xác nhận trước đó.");
                }

                if (order.Status == -1)
                {
                    return (false, "Đơn hàng đã bị hủy, không thể xử lý.");
                }

                var dbTransaction = await _context.Transactions
                    .FirstOrDefaultAsync(t => t.OrderId == orderId);

                if (dbTransaction == null)
                    return (false, "Không tìm thấy giao dịch liên quan");

                if (newStatus == "Thành công")
                {
                    order.Status = 1;
                    dbTransaction.Status = TransactionStatus.Success;

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return (true, "Xác nhận giao hàng thành công!");
                }
                else if (newStatus == "Đã hủy")
                {
                    order.Status = -1;
                    dbTransaction.Status = TransactionStatus.Canceled;

                    var user = await _context.Users.FindAsync(order.UserId);
                    if (user != null)
                    {
                        user.WalletBalance += order.TotalAmount ?? 0;

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
                        await _context.SaveChangesAsync();

                        refundTrans.TransactionCode = $"{TransactionPrefixes.Refund}-{refundTrans.TransactionId:D4}";
                    }

                
                    foreach (var detail in order.OrderDetails)
                    {
                        var product = await _context.Products.FindAsync(detail.ProductId);
                        if (product != null)
                        {
                            product.StockQuantity += detail.Quantity;
                        }
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return (true, "Đã hủy đơn và hoàn tiền thành công!");
                }

                return (false, "Trạng thái không hợp lệ");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return (false, $"Lỗi: {ex.Message}");
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

        public async Task AutoCancelExpiredBookingsAsync()
        {
            try
            {
                // Tìm các PitchSlots có trạng thái PendingConfirm (1)
                // Và thời gian bắt đầu + 30 phút < Hiện tại
                var now = DateTime.Now;
                
                var expiredBookings = await _context.PitchSlots
                    .Include(ps => ps.TimeSlot)
                    .Where(ps => ps.Status == BookingStatus.PendingConfirm)
                    .ToListAsync();

                var codesToCancel = expiredBookings
                    .Where(ps => ps.PlayDate.Date.Add(ps.TimeSlot.StartTime).AddMinutes(30) < now)
                    .Select(ps => ps.BookingCode)
                    .Where(code => !string.IsNullOrEmpty(code))
                    .ToList();

                if (!codesToCancel.Any()) return;


            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error auto-canceling bookings: {ex.Message}");
            }
        }

        public async Task<TransactionDetailDTO?> GetTransactionDetailsAsync(string code)
        {
            var trans = await _context.Transactions
                .Include(t => t.Sender)
                .Include(t => t.Order)
                    .ThenInclude(o => o.OrderDetails)
                        .ThenInclude(od => od.Product)
                .Include(t => t.Booking)
                    .ThenInclude(b => b.Pitch)
                .FirstOrDefaultAsync(t => t.TransactionCode == code);

            if (trans == null) return null;

            var result = new TransactionDetailDTO
            {
                Code = trans.TransactionCode,
                User = trans.Sender?.FullName ?? trans.Sender?.Username,
                Phone = trans.Sender?.Phone,
                Amount = trans.Amount,
                BalanceAfter = trans.BalanceAfter,
                Date = trans.TransactionDate,
                Type = trans.TransactionType,
                Status = trans.Status,
                Source = trans.Source,
                Message = trans.Message
            };

            if (trans.Order != null)
            {
                result.Items = trans.Order.OrderDetails.Select(od => new OrderItemDetailDTO
                {
                    ProductName = od.Product?.ProductName,
                    Quantity = od.Quantity,
                    Price = od.Price
                }).ToList();
            }

            if (trans.Booking != null)
            {
                result.Booking = new BookingInfoDetailDTO
                {
                    PitchName = trans.Booking.Pitch?.PitchName,
                    StartTime = trans.Booking.StartTime,
                    EndTime = trans.Booking.EndTime,
                    TotalPrice = trans.Booking.TotalPrice ?? 0
                };
            }
            else if (trans.TransactionType == TransactionTypes.Booking || trans.TransactionType == TransactionTypes.Refund || trans.TransactionType == TransactionTypes.RefundBooking)
            {
                // Thử tìm trong PitchSlots nếu Booking null
                var slot = await _context.PitchSlots
                    .Include(ps => ps.Pitch)
                    .Include(ps => ps.TimeSlot)
                    .FirstOrDefaultAsync(ps => ps.BookingCode == trans.TransactionCode);

                if (slot != null)
                {
                    result.Booking = new BookingInfoDetailDTO
                    {
                        PitchName = slot.Pitch?.PitchName,
                        StartTime = slot.PlayDate.Date.Add(slot.TimeSlot.StartTime),
                        EndTime = slot.PlayDate.Date.Add(slot.TimeSlot.EndTime),
                        TotalPrice = trans.Amount // Lấy tạm Amount từ giao dịch
                    };
                }
            }

            return result;
        }
    }
}