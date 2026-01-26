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

        // Giao dịch chung
        public async Task<(List<UserTransactionDTO> Data, int TotalRecords)> LoadUserTransactionAsync(int userId, int page, int pageSize)
        {
            var query = _context.Transactions
                .Where(t => t.UserId == userId
                && t.TransactionType != "Chuyển tiền"
                    && t.TransactionType != "Nhận tiền"
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

        // Lịch sử đặt sân
        public async Task<(List<UserBookingDTO> Data, int TotalRecords)> LoadUserBookingsAsync(int userId, int page, int pageSize)
        {
            var query = _context.Bookings
                .Include(b => b.Pitch)
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.BookingDate);

            var totalRecords = await query.CountAsync();

            var data = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(b => new UserBookingDTO
                {
                    BookingCode = "BK" + b.BookingId.ToString("D4"),
                    PitchName = b.Pitch.PitchName,
                    BookingDate = b.BookingDate
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

                // Update số dư
                sender.WalletBalance -= dto.Amount;
                receiver.WalletBalance += dto.Amount;

                // Message
                string senderMsg;
                string receiverMsg;

                if (!string.IsNullOrEmpty(dto.Message))
                {
                    // TRƯỜNG HỢP 1: Có nhập lời nhắn -> Lấy đúng lời nhắn đó
                    senderMsg = dto.Message;

                    // Với người nhận, nên kèm tên người gửi để họ biết ai chuyển + lời nhắn
                    receiverMsg = dto.Message;
                }
                else
                {
                    // TRƯỜNG HỢP 2: Không nhập gì -> Tự sinh nội dung mặc định
                    senderMsg = $"Chuyển {dto.Amount:N0}₫ cho {receiver.FullName}";
                    receiverMsg = $"Nhận {dto.Amount:N0}₫ từ {sender.FullName}";
                }

                // Cắt chuỗi nếu quá dài (để tránh lỗi database)
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
        //hàm load cho tab lịch sử chuyển tiền
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