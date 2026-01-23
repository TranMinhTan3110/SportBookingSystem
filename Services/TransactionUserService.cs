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

            public async Task<List<UserTransactionDTO>> LoadUserTransactionAsync(int userId)
            {
                return await _context.Transactions
                    .Where(t => t.UserId == userId)
                    .OrderByDescending(t => t.TransactionDate)
                    .Select(t => new UserTransactionDTO
                    {
                        TransactionCode = t.TransactionCode,
                        Amount = t.Amount,
                        IsPositive = t.TransactionType == TransactionTypes.Recharge ||
                                     t.TransactionType == TransactionTypes.Refund,
                        TransactionType = t.TransactionType,
                        TransactionSource = t.Source, // Thêm source
                        Date = t.TransactionDate,
                        Status = t.Status,
                        Message = t.Message
                    })
                    .ToListAsync();
            }

            public async Task<List<UserBookingDTO>> LoadUserBookingsAsync(int userId)
            {
                return await _context.Bookings
                    .Include(b => b.Pitch)
                    .Where(b => b.UserId == userId)
                    .OrderByDescending(b => b.BookingDate)
                    .Select(b => new UserBookingDTO
                    {
                        BookingCode = "BK" + b.BookingId.ToString("D4"),
                        PitchName = b.Pitch.PitchName,
                        BookingDate = b.BookingDate
                        //StartTime = b.StartTime,
                        //EndTime = b.EndTime,
                        //TotalAmount = b.TotalAmount,
                        //Status = b.Status
                    })
                    .ToListAsync();
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

                // ✅ MÃ GIAO DỊCH NGẮN (< 50 ký tự)
                var timestamp = DateTime.Now.ToString("yyMMddHHmmss"); // 12 ký tự
                string senderCode = $"TRF-O{timestamp}{senderId}";      // TRF-O250124102233-1
                string receiverCode = $"TRF-I{timestamp}{receiver.UserId}"; // TRF-I250124102233-2

                // Update số dư
                sender.WalletBalance -= dto.Amount;
                receiver.WalletBalance += dto.Amount;

                // Message
                string senderMsg = $"Chuyển {dto.Amount:N0}₫ cho {receiver.FullName}";
                string receiverMsg = $"Nhận {dto.Amount:N0}₫ từ {sender.FullName}";

                if (!string.IsNullOrEmpty(dto.Message))
                {
                    senderMsg += $" - {dto.Message}";
                    receiverMsg += $" - {dto.Message}";
                }

                if (senderMsg.Length > 500) senderMsg = senderMsg.Substring(0, 497) + "...";
                if (receiverMsg.Length > 500) receiverMsg = receiverMsg.Substring(0, 497) + "...";

                // Giao dịch người GỬI
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

                // Giao dịch người NHẬN
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
    }
    }