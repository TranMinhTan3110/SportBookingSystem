using Microsoft.EntityFrameworkCore;
using SportBookingSystem.Constants;
using SportBookingSystem.DTO;
using SportBookingSystem.Models.EF;

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
    }
}