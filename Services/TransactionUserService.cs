using Microsoft.EntityFrameworkCore;
using SportBookingSystem.Constants;
using SportBookingSystem.DTO;
using SportBookingSystem.Models.EF;
using SportBookingSystem.Models.Entities;

namespace SportBookingSystem.Services
{
    public class TransactionUserService : ITransactionUserService
    {
        private readonly ApplicationDbContext _context;

        public TransactionUserService(ApplicationDbContext context)
        {
            _context = context;
        }

        //giao dịch chung

        public async Task<(List<UserTransactionDTO> Data, int TotalRecords)> LoadUserTransactionAsync(int userId, int page, int pageSize)
        {
            var query = _context.Transactions
                .Where(t => t.UserId == userId
                    && t.TransactionType != "Chuyển tiền"
                    && t.TransactionType != "Nhận tiền"
                    && t.TransactionType != "Thanh toán Booking" 
                )
                .OrderByDescending(t => t.TransactionDate);

            var totalRecords = await query.CountAsync();

            var data = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(t => new UserTransactionDTO
                {
                    TransactionCode = t.TransactionCode,
                    Amount = t.Amount,
                    IsPositive = t.TransactionType == TransactionTypes.Recharge ||
                                 t.TransactionType == TransactionTypes.Refund,
                    TransactionType = t.TransactionType,
                    TransactionSource = t.Source,
                    Date = t.TransactionDate,
                    Status = t.Status,
                    Message = t.Message
                })
                .ToListAsync();

            return (data, totalRecords);
        }

        // ============================================
        // LỊCH SỬ ĐẶT SÂN (CHỈ LẤY BOOKING)
        // ============================================
        public async Task<(List<UserBookingDTO> Data, int TotalRecords)> LoadUserBookingsAsync(int userId, int page, int pageSize)
        {
            // Lấy danh sách Booking từ bảng PitchSlots (hoặc Bookings nếu bạn có)
            var query = _context.PitchSlots
                .Include(ps => ps.Pitch)
                .Include(ps => ps.TimeSlot)
                .Where(ps => ps.Status == 1)  // Status = 1 nghĩa là đã đặt
                .OrderByDescending(ps => ps.PlayDate);

            // Lọc theo UserId nếu có quan hệ
            // Nếu PitchSlots không có UserId, phải JOIN với Transactions
            var transactionCodes = await _context.Transactions
                .Where(t => t.UserId == userId && t.TransactionType == "Thanh toán Booking")
                .Select(t => t.TransactionCode)
                .ToListAsync();

            var filteredQuery = query.Where(ps => transactionCodes.Contains(ps.BookingCode));

            var totalRecords = await filteredQuery.CountAsync();

            var data = await filteredQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(ps => new UserBookingDTO
                {
                    BookingCode = ps.BookingCode ?? "N/A",
                    PitchName = ps.Pitch.PitchName,
                    BookingDate = ps.PlayDate,
                    StartTime = ps.PlayDate.Add(ps.TimeSlot.StartTime),
                    EndTime = ps.PlayDate.Add(ps.TimeSlot.EndTime),
                    TimeRange = ps.TimeSlot.StartTime.ToString(@"hh\:mm") + " - " + ps.TimeSlot.EndTime.ToString(@"hh\:mm"),
                    TotalPrice = ps.Pitch.PricePerHour * 1.5m,  // Tạm tính
                    Status = ps.Status,
                    CheckInCode = ps.BookingCode
                })
                .ToListAsync();

            return (data, totalRecords);
        }

        // Kiểm tra người nhận theo SĐT
        public async Task<Users> GetUserByPhoneAsync(string phoneNumber)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Phone == phoneNumber);
        }

        // Xử lý chuyển tiền
        public async Task<string> TransferMoneyAsync(int senderId, TransferDTO dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                if (dto.Amount < 1000)
                    return "Số tiền tối thiểu là 1,000₫";

                var sender = await _context.Users.FindAsync(senderId);
                if (sender == null)
                    return "Không tìm thấy người gửi";

                if (sender.WalletBalance < dto.Amount)
                    return "Số dư không đủ";

                var receiver = await _context.Users
                    .FirstOrDefaultAsync(u => u.Phone == dto.ReceiverPhone);

                if (receiver == null)
                    return "Không tìm thấy người nhận";

                if (receiver.UserId == senderId)
                    return "Không thể chuyển tiền cho chính mình";

                var timestamp = DateTime.Now.ToString("yyMMddHHmmss");
                string senderCode = $"TRF-O{timestamp}{senderId}";
                string receiverCode = $"TRF-I{timestamp}{receiver.UserId}";

                sender.WalletBalance -= dto.Amount;
                receiver.WalletBalance += dto.Amount;

                string senderMsg;
                string receiverMsg;

                if (!string.IsNullOrEmpty(dto.Message))
                {
                    senderMsg = dto.Message;
                    receiverMsg = dto.Message;
                }
                else
                {
                    senderMsg = $"Chuyển {dto.Amount:N0}₫ cho {receiver.FullName}";
                    receiverMsg = $"Nhận {dto.Amount:N0}₫ từ {sender.FullName}";
                }

                if (senderMsg.Length > 500) senderMsg = senderMsg.Substring(0, 497) + "...";
                if (receiverMsg.Length > 500) receiverMsg = receiverMsg.Substring(0, 497) + "...";

                _context.Transactions.Add(new Transactions
                {
                    UserId = senderId,
                    TransactionCode = senderCode,
                    Amount = dto.Amount,
                    TransactionType = "Chuyển tiền",
                    Source = "Ví nội bộ",
                    Status = "Thành công",
                    Message = senderMsg,
                    TransactionDate = DateTime.Now,
                    ReceiverId = receiver.UserId,
                    BalanceAfter = sender.WalletBalance
                });

                _context.Transactions.Add(new Transactions
                {
                    UserId = receiver.UserId,
                    TransactionCode = receiverCode,
                    Amount = dto.Amount,
                    TransactionType = "Nhận tiền",
                    Source = "Ví nội bộ",
                    Status = "Thành công",
                    Message = receiverMsg,
                    TransactionDate = DateTime.Now,
                    ReceiverId = senderId,
                    BalanceAfter = receiver.WalletBalance
                });

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return "success";
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Console.WriteLine($"Error: {ex.InnerException?.Message ?? ex.Message}");
                return $"Lỗi: {ex.InnerException?.Message ?? ex.Message}";
            }
        }

        // LỊCH SỬ CHUYỂN TIỀN
        public async Task<(List<UserTransferDTO> Data, int TotalRecords)> LoadUserTransfersAsync(int userId, int page, int pageSize)
        {
            var query = _context.Transactions
                .Include(t => t.Sender)
                .Include(t => t.Receiver)
                .Where(t => t.UserId == userId
                         && (t.TransactionType == "Chuyển tiền" || t.TransactionType == "Nhận tiền"))
                .OrderByDescending(t => t.TransactionDate);

            var totalRecords = await query.CountAsync();

            var data = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(t => new UserTransferDTO
                {
                    TransactionCode = t.TransactionCode,
                    Amount = t.Amount,
                    IsSender = t.UserId == userId,
                    SenderName = t.Sender.FullName ?? t.Sender.Username,
                    ReceiverName = t.Receiver != null ? (t.Receiver.FullName ?? t.Receiver.Username) : "N/A",
                    Date = t.TransactionDate,
                    Status = t.Status,
                    Message = t.Message
                })
                .ToListAsync();

            return (data, totalRecords);
        }
    }
}