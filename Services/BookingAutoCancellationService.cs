using SportBookingSystem.Services;
using SportBookingSystem.Models.EF;
using Microsoft.EntityFrameworkCore;
using SportBookingSystem.Constants;

namespace SportBookingSystem.Services
{
    public class BookingAutoCancellationService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(5);

        public BookingAutoCancellationService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                        var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();

                        var now = DateTime.Now;
                        var expiredBookings = await context.PitchSlots
                            .Include(ps => ps.TimeSlot)
                            .Where(ps => ps.Status == BookingStatus.PendingConfirm)
                            .ToListAsync(stoppingToken);

                        var codesToCancel = expiredBookings
                            .Where(ps => ps.PlayDate.Date.Add(ps.TimeSlot.StartTime).AddMinutes(30) < now)
                            .Select(ps => ps.BookingCode)
                            .Where(code => !string.IsNullOrEmpty(code))
                            .ToList();

                        if (codesToCancel.Any())
                        {
                            var slotsToCancel = expiredBookings.Where(ps => codesToCancel.Contains(ps.BookingCode)).ToList();
                        
                        var transactionsToCancel = await context.Transactions
                            .Where(t => codesToCancel.Contains(t.TransactionCode) && t.TransactionType == TransactionTypes.Booking)
                            .ToListAsync(stoppingToken);

                        using var dbTransaction = await context.Database.BeginTransactionAsync(stoppingToken);
                        
                        foreach (var slot in slotsToCancel)
                        {
                            Console.WriteLine($"[AutoCancel] Hủy đặt sân quá hạn: {slot.BookingCode}");
                            
                            var durationInMinutes = (slot.TimeSlot.EndTime - slot.TimeSlot.StartTime).TotalMinutes;
                            var halfDuration = durationInMinutes / 2;
                            var matchStartTime = slot.PlayDate.Date.Add(slot.TimeSlot.StartTime);

                            if (now < matchStartTime.AddMinutes(halfDuration))
                            {
                                context.PitchSlots.Remove(slot);
                            }
                            else
                            {
                                slot.Status = BookingStatus.RefundBooking;
                                slot.UpdatedAt = now;
                                context.PitchSlots.Update(slot);
                            }

                            var originalTrans = transactionsToCancel.FirstOrDefault(t => t.TransactionCode == slot.BookingCode);
                            if (originalTrans != null)
                            {
                                originalTrans.Status = TransactionStatus.CancelBooking;
                                context.Transactions.Update(originalTrans);
                            }
                        }

                        await context.SaveChangesAsync(stoppingToken);
                        await dbTransaction.CommitAsync(stoppingToken);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error in BookingAutoCancellationService: {ex.Message}");
                }

                await Task.Delay(_checkInterval, stoppingToken);
            }
        }
    }
}
