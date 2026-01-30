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

                        foreach (var code in codesToCancel)
                        {
                            Console.WriteLine($"[AutoCancel] Hủy đặt sân quá hạn: {code}");
                            await bookingService.CancelBookingAsync(code!, isAutoCancel: true);
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
