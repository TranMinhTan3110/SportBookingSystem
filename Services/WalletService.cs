using Microsoft.EntityFrameworkCore;
using SportBookingSystem.Constants;
using SportBookingSystem.Models.EF;
using SportBookingSystem.Models.ViewModels;

namespace SportBookingSystem.Services
{
    public class WalletService : IWalletService
    {
        private readonly ApplicationDbContext _context;

        public WalletService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Transactions> CreateRechargeTransactionAsync(int userId, decimal amount, string txnRef)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                throw new Exception("Không tìm thấy người dùng");
            }

            var transaction = new Transactions
            {
                TransactionCode = txnRef,
                Amount = amount,
                Message = "Nạp tiền vào ví qua VNPay",
                TransactionType = TransactionTypes.Recharge,
                Source = TransactionSources.VNPay,
                Status = TransactionStatus.Pending,
                TransactionDate = DateTime.Now,
                UserId = userId,
                BalanceAfter = user.WalletBalance
            };

            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();

            return transaction;
        }

        public async Task<(bool success, string message)> ExecuteRechargeAsync(VnPayResponseModel vnPayResponse)
        {
            if (!vnPayResponse.IsValidSignature)
            {
                return (false, "Chữ ký không hợp lệ");
            }

            if (vnPayResponse.ResponseCode != "00")
            {
                return (false, GetVnPayResponseMessage(vnPayResponse.ResponseCode));
            }

            var transaction = await _context.Transactions
                .Include(t => t.Sender)
                .FirstOrDefaultAsync(t => t.TransactionCode == vnPayResponse.TransactionCode);

            if (transaction == null)
            {
                return (false, "Không tìm thấy giao dịch");
            }

            if (transaction.Status == TransactionStatus.Success)
            {
                return (false, "Giao dịch đã được xử lý trước đó");
            }

            using var dbTransaction = await _context.Database.BeginTransactionAsync();
            try
            {
                transaction.Sender.WalletBalance += transaction.Amount;

                transaction.Status = TransactionStatus.Success;
                transaction.BalanceAfter = transaction.Sender.WalletBalance;
                transaction.Message = $"Nạp tiền thành công qua VNPay - Mã GD: {vnPayResponse.TransactionNo}";

                await _context.SaveChangesAsync();
                await dbTransaction.CommitAsync();

                return (true, $"Nạp tiền thành công {transaction.Amount:N0}₫ vào ví");
            }
            catch (Exception ex)
            {
                await dbTransaction.RollbackAsync();
                return (false, $"Lỗi xử lý giao dịch: {ex.Message}");
            }
        }

        private string GetVnPayResponseMessage(string responseCode)
        {
            return responseCode switch
            {
                "00" => "Giao dịch thành công",
                "07" => "Trừ tiền thành công. Giao dịch bị nghi ngờ (liên quan tới lừa đảo, giao dịch bất thường).",
                "09" => "Giao dịch không thành công do: Thẻ/Tài khoản của khách hàng chưa đăng ký dịch vụ InternetBanking tại ngân hàng.",
                "10" => "Giao dịch không thành công do: Khách hàng xác thực thông tin thẻ/tài khoản không đúng quá 3 lần",
                "11" => "Giao dịch không thành công do: Đã hết hạn chờ thanh toán. Xin quý khách vui lòng thực hiện lại giao dịch.",
                "12" => "Giao dịch không thành công do: Thẻ/Tài khoản của khách hàng bị khóa.",
                "13" => "Giao dịch không thành công do Quý khách nhập sai mật khẩu xác thực giao dịch (OTP).",
                "24" => "Giao dịch không thành công do: Khách hàng hủy giao dịch",
                "51" => "Giao dịch không thành công do: Tài khoản của quý khách không đủ số dư để thực hiện giao dịch.",
                "65" => "Giao dịch không thành công do: Tài khoản của Quý khách đã vượt quá hạn mức giao dịch trong ngày.",
                "75" => "Ngân hàng thanh toán đang bảo trì.",
                "79" => "Giao dịch không thành công do: KH nhập sai mật khẩu thanh toán quá số lần quy định.",
                _ => $"Giao dịch thất bại - Mã lỗi: {responseCode}"
            };
        }
    }
}