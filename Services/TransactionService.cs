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
    }
}