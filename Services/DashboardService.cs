using Microsoft.EntityFrameworkCore;
using SportBookingSystem.Constants;
using SportBookingSystem.Models.EF;
using SportBookingSystem.Models.ViewModels;

namespace SportBookingSystem.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly ApplicationDbContext _context;

        public DashboardService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<DashboardViewModel> GetDashboardDataAsync()
        {
            var viewModel = new DashboardViewModel();

            try
            {
                var totalTrans = await _context.Transactions.CountAsync();

                // Lấy doanh thu hôm nay
                var revenueData = await GetTodayRevenueAsync();
                viewModel.TodayRevenue = revenueData.todayRevenue;
                viewModel.RevenueGrowthPercent = revenueData.growthPercent;
                viewModel.IsRevenueGrowthPositive = revenueData.isPositive;

                // Lấy số lượt đặt sân hôm nay
                var bookingData = await GetTodayBookingsCountAsync();
                viewModel.TodayBookingsCount = bookingData.todayCount;
                viewModel.BookingGrowthPercent = bookingData.growthPercent;
                viewModel.IsBookingGrowthPositive = bookingData.isPositive;

                // Lấy số sân đang hoạt động
                var pitchData = await GetActivePitchesCountAsync();
                viewModel.ActivePitchesCount = pitchData.activeCount;
                viewModel.TotalPitchesCount = pitchData.totalCount;

                // Lấy dữ liệu biểu đồ 7 ngày
                viewModel.RevenueChart = await GetLast7DaysRevenueAsync();

                // Lấy top khách hàng
                viewModel.TopCustomers = await GetTopCustomersAsync(5);

                // Lấy lịch đặt sân hôm nay
                viewModel.TodayBookings = await GetTodayBookingsScheduleAsync();

                // Lấy top sân được đặt nhiều nhất
                viewModel.TopPitches = await GetTopPitchesAsync(4);

                Console.WriteLine($"Dashboard loaded: Revenue={viewModel.TodayRevenue}, Bookings={viewModel.TodayBookingsCount}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"!!! ERROR in GetDashboardDataAsync: {ex.Message}");
                Console.WriteLine($"Stack: {ex.StackTrace}");
            }

            return viewModel;
        }

        public async Task<(decimal todayRevenue, decimal growthPercent, bool isPositive)> GetTodayRevenueAsync()
        {
            try
            {
                var today = DateTime.Today;
                var tomorrow = today.AddDays(1);
                var yesterday = today.AddDays(-1);

                // Doanh thu hôm nay = (Thanh toán Booking + Thanh toán Order) - (Hoàn tiền + Hoàn tiền đặt sân)
                var todayIncome = await _context.Transactions
                    .Where(t => t.TransactionDate >= today
                        && t.TransactionDate < tomorrow
                        && t.Status.Trim() != "Chờ xác nhận"
                        && (t.TransactionType.Trim() == "Thanh toán Booking" || t.TransactionType.Trim() == "Thanh toán Order"))
                    .SumAsync(t => t.Amount);

                var todayRefund = await _context.Transactions
                    .Where(t => t.TransactionDate >= today
                        && t.TransactionDate < tomorrow
                        && t.Status.Trim() != "Chờ xác nhận"
                        && (t.TransactionType.Trim() == "Hoàn tiền" || t.TransactionType.Trim() == "Hoàn tiền đặt sân"))
                    .SumAsync(t => t.Amount);

                var todayRevenue = todayIncome - todayRefund;

                // Doanh thu hôm qua = (Thanh toán Booking + Thanh toán Order) - (Hoàn tiền + Hoàn tiền đặt sân)
                var yesterdayIncome = await _context.Transactions
                    .Where(t => t.TransactionDate >= yesterday
                        && t.TransactionDate < today
                        && t.Status.Trim() != "Chờ xác nhận"
                        && (t.TransactionType.Trim() == "Thanh toán Booking" || t.TransactionType.Trim() == "Thanh toán Order"))
                    .SumAsync(t => t.Amount);

                var yesterdayRefund = await _context.Transactions
                    .Where(t => t.TransactionDate >= yesterday
                        && t.TransactionDate < today
                        && t.Status.Trim() != "Chờ xác nhận"
                        && (t.TransactionType.Trim() == "Hoàn tiền" || t.TransactionType.Trim() == "Hoàn tiền đặt sân"))
                    .SumAsync(t => t.Amount);

                var yesterdayRevenue = yesterdayIncome - yesterdayRefund;

                // Tính tỷ lệ tăng trưởng
                decimal growthPercent = 0;
                bool isPositive = true;

                if (yesterdayRevenue > 0)
                {
                    growthPercent = Math.Round(((todayRevenue - yesterdayRevenue) / yesterdayRevenue) * 100, 1);
                    isPositive = growthPercent >= 0;
                }
                else if (todayRevenue > 0)
                {
                    growthPercent = 100;
                    isPositive = true;
                }

                return (todayRevenue, Math.Abs(growthPercent), isPositive);
            }
            catch (Exception ex)
            {
                return (0, 0, true);
            }
        }

        public async Task<(int todayCount, decimal growthPercent, bool isPositive)> GetTodayBookingsCountAsync()
        {
            try
            {
                var today = DateTime.Today;
                var tomorrow = today.AddDays(1);
                var yesterday = today.AddDays(-1);

                var todayCount = await _context.Transactions
                    .Where(t => t.TransactionDate >= today
                        && t.TransactionDate < tomorrow
                        && t.TransactionType.Trim() == "Thanh toán Booking"
                        && t.Status.Trim() == "Thành công")
                    .CountAsync();


                // Đếm số lượt đặt sân hôm qua
                var yesterdayCount = await _context.Transactions
                    .Where(t => t.TransactionDate >= yesterday
                        && t.TransactionDate < today
                        && t.TransactionType.Trim() == "Thanh toán Booking"
                        && t.Status.Trim() == "Thành công")
                    .CountAsync();


                // Tính tỷ lệ tăng trưởng
                decimal growthPercent = 0;
                bool isPositive = true;

                if (yesterdayCount > 0)
                {
                    growthPercent = Math.Round(((decimal)(todayCount - yesterdayCount) / yesterdayCount) * 100, 1);
                    isPositive = growthPercent >= 0;
                }
                else if (todayCount > 0)
                {
                    growthPercent = 100;
                    isPositive = true;
                }

                return (todayCount, Math.Abs(growthPercent), isPositive);
            }
            catch (Exception ex)
            {
                return (0, 0, true);
            }
        }

        public async Task<(int activeCount, int totalCount)> GetActivePitchesCountAsync()
        {
            try
            {
                var totalCount = await _context.Pitches.CountAsync();
                var activeCount = await _context.Pitches
                    .Where(p => p.Status != "Bảo trì")
                    .CountAsync();

                return (activeCount, totalCount);
            }
            catch (Exception ex)
            {
                return (0, 0);
            }
        }

        public async Task<List<RevenueChartData>> GetLast7DaysRevenueAsync()
        {
            var chartData = new List<RevenueChartData>();

            try
            {
                for (int i = 6; i >= 0; i--)
                {
                    var date = DateTime.Today.AddDays(-i);
                    var nextDay = date.AddDays(1);

                    // Doanh thu = (Thanh toán Booking + Thanh toán Order) - (Hoàn tiền + Hoàn tiền đặt sân)
                    var income = await _context.Transactions
                        .Where(t => t.TransactionDate >= date
                            && t.TransactionDate < nextDay
                            && t.Status.Trim() != "Chờ xác nhận"
                            && (t.TransactionType.Trim() == "Thanh toán Booking" || t.TransactionType.Trim() == "Thanh toán Order"))
                        .SumAsync(t => t.Amount);

                    var refund = await _context.Transactions
                        .Where(t => t.TransactionDate >= date
                            && t.TransactionDate < nextDay
                            && t.Status.Trim() != "Chờ xác nhận"
                            && (t.TransactionType.Trim() == "Hoàn tiền" || t.TransactionType.Trim() == "Hoàn tiền đặt sân"))
                        .SumAsync(t => t.Amount);

                    var revenue = income - refund;

                    var dayOfWeek = GetVietnameseDayOfWeek(date.DayOfWeek);

                    chartData.Add(new RevenueChartData
                    {
                        Date = date.ToString("yyyy-MM-dd"),
                        DayOfWeek = dayOfWeek,
                        Revenue = revenue
                    });

                    Console.WriteLine($"  {date:dd/MM} ({dayOfWeek}): {revenue:N0}₫");
                }
            }
            catch (Exception ex)
            {
            }

            return chartData;
        }

        public async Task<List<TopCustomerData>> GetTopCustomersAsync(int topCount = 5)
        {
            try
            {
                var startOfMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                var endOfMonth = startOfMonth.AddMonths(1);

                var topCustomers = await _context.Transactions
                    .Where(t => t.TransactionDate >= startOfMonth
                        && t.TransactionDate < endOfMonth
                        && t.TransactionType.Trim() == "Thanh toán Booking"
                        && t.Status.Trim() != "Chờ xác nhận"
                        && t.Sender != null)
                    .GroupBy(t => new { t.UserId, t.Sender.FullName, t.Sender.Username })
                    .Select(g => new TopCustomerData
                    {
                        CustomerName = g.Key.FullName ?? g.Key.Username,
                        BookingCount = g.Count(),
                        TotalAmount = g.Sum(t => t.Amount)
                    })
                    .OrderByDescending(c => c.BookingCount)
                    .Take(topCount)
                    .ToListAsync();

                foreach (var c in topCustomers)
                {
                }

                return topCustomers;
            }
            catch (Exception ex)
            {
                return new List<TopCustomerData>();
            }
        }

        public async Task<List<TodayBookingData>> GetTodayBookingsScheduleAsync()
        {
            try
            {
                var today = DateTime.Today;
                var tomorrow = today.AddDays(1);

                var todayBookings = await _context.Bookings
                    .Include(b => b.Pitch)
                    .Include(b => b.User)
                    .Where(b => b.BookingDate >= today && b.BookingDate < tomorrow)
                    .OrderBy(b => b.StartTime)
                    .ToListAsync();

                var result = todayBookings.Select(b => new TodayBookingData
                {
                    BookingId = b.BookingId,
                    StartTime = b.StartTime.ToString("HH:mm"),
                    PitchName = b.Pitch.PitchName,
                    CustomerName = b.User != null ? (b.User.FullName ?? b.User.Username) : "Khách vãng lai",
                    StatusText = GetBookingStatusText(b.Status),
                    StatusClass = GetBookingStatusClass(b.Status)
                }).ToList();

                Console.WriteLine($"Found {result.Count} bookings today");

                return result;
            }
            catch (Exception ex)
            {
                return new List<TodayBookingData>();
            }
        }

        public async Task<List<TopPitchData>> GetTopPitchesAsync(int topCount = 4)
        {
            try
            {
                var startOfMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                var endOfMonth = startOfMonth.AddMonths(1);
                var transactions = await _context.Transactions
                    .Where(t => t.TransactionDate >= startOfMonth
                        && t.TransactionDate < endOfMonth
                        && t.TransactionType.Trim() == "Thanh toán Booking"
                        && t.Status.Trim() == "Thành công"
                        && !string.IsNullOrEmpty(t.Message))
                    .Select(t => new
                    {
                        t.Message,
                        t.Amount
                    })
                    .ToListAsync();

                var allPitches = await _context.Pitches.ToListAsync();

                var pitchBookings = new Dictionary<int, (string pitchName, int count, decimal revenue)>();

                foreach (var trans in transactions)
                {
                    string message = trans.Message;

                    string extractedPitchName = null;
                    int matchedPitchId = -1;

                    foreach (var pitch in allPitches)
                    {
                        if (message.Contains(pitch.PitchName, StringComparison.OrdinalIgnoreCase))
                        {
                            extractedPitchName = pitch.PitchName;
                            matchedPitchId = pitch.PitchId;
                            break;
                        }
                    }

                    if (matchedPitchId != -1)
                    {
                        if (!pitchBookings.ContainsKey(matchedPitchId))
                        {
                            pitchBookings[matchedPitchId] = (extractedPitchName, 0, 0);
                        }

                        var current = pitchBookings[matchedPitchId];
                        pitchBookings[matchedPitchId] = (
                            current.pitchName,
                            current.count + 1,
                            current.revenue + trans.Amount
                        );
                    }
                }

                var topPitches = pitchBookings
                    .Select(pb => new TopPitchData
                    {
                        PitchName = pb.Value.pitchName,
                        BookingCount = pb.Value.count,
                        TotalRevenue = pb.Value.revenue
                    })
                    .OrderByDescending(p => p.BookingCount)
                    .Take(topCount)
                    .ToList();

                for (int i = 0; i < topPitches.Count; i++)
                {
                    topPitches[i].Rank = i + 1;
                }

                Console.WriteLine($"Top {topPitches.Count} pitches:");
                foreach (var p in topPitches)
                {
                    Console.WriteLine($"  {p.Rank}. {p.PitchName}: {p.BookingCount} bookings, {p.TotalRevenue:N0}₫");
                }

                return topPitches;
            }
            catch (Exception ex)
            {
                return new List<TopPitchData>();
            }
        }

        #region Private Helper Methods

        private string GetVietnameseDayOfWeek(DayOfWeek dayOfWeek)
        {
            return dayOfWeek switch
            {
                DayOfWeek.Monday => "T2",
                DayOfWeek.Tuesday => "T3",
                DayOfWeek.Wednesday => "T4",
                DayOfWeek.Thursday => "T5",
                DayOfWeek.Friday => "T6",
                DayOfWeek.Saturday => "T7",
                DayOfWeek.Sunday => "CN",
                _ => ""
            };
        }

        private string GetBookingStatusText(int status)
        {
            return status switch
            {
                BookingStatus.PendingConfirm => "Chờ xác nhận",
                BookingStatus.CheckedIn => "Đã check-in",
                BookingStatus.Completed => "Hoàn thành",
                BookingStatus.Cancelled => "Đã hủy",
                BookingStatus.RefundBooking => "Đã hoàn tiền",
                _ => "Không xác định"
            };
        }

        private string GetBookingStatusClass(int status)
        {
            return status switch
            {
                BookingStatus.PendingConfirm => "pending",
                BookingStatus.CheckedIn => "confirmed",
                BookingStatus.Completed => "completed",
                BookingStatus.Cancelled => "cancelled",
                BookingStatus.RefundBooking => "cancelled",
                _ => "pending"
            };
        }

        #endregion
    }
}