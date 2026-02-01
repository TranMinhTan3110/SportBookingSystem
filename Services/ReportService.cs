using Microsoft.EntityFrameworkCore;
using SportBookingSystem.Models.EF;
using SportBookingSystem.Models.ViewModels;
using SportBookingSystem.Constants;

namespace SportBookingSystem.Services
{
    public class ReportService : IReportService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ReportService> _logger;

        public ReportService(ApplicationDbContext context, ILogger<ReportService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<ReportDataViewModel> GetReportDataAsync(DateTime fromDate, DateTime toDate)
        {
            try
            {
                // QUAN TRỌNG: Normalize dates
                // fromDate: Bắt đầu từ 00:00:00
                // toDate: Kết thúc đến 23:59:59.9999999
                fromDate = fromDate.Date; // 2025-12-31 00:00:00
                toDate = toDate.Date.AddDays(1).AddTicks(-1); // 2026-01-30 23:59:59.9999999

                _logger.LogInformation("=== BÁO CÁO TỪ {FromDate} ĐẾN {ToDate} ===", fromDate, toDate);

                var bookingRevenue = await CalculateBookingRevenueAsync(fromDate, toDate);
                var serviceRevenue = await CalculateServiceRevenueAsync(fromDate, toDate);
                var topSports = await GetTopSportsAsync(fromDate, toDate);
                var growthRate = await CalculateGrowthRateAsync(fromDate, toDate);

                _logger.LogInformation("Tổng doanh thu: {Total}₫ (Booking: {Booking}₫ + Service: {Service}₫)",
                    bookingRevenue + serviceRevenue, bookingRevenue, serviceRevenue);

                // Mặc định hiển thị 3 tháng gần nhất trong bảng Monthly Summary
                var monthlySummaries = await GetMonthlySummariesAsync(3);

                return new ReportDataViewModel
                {
                    TotalRevenue = bookingRevenue + serviceRevenue,
                    BookingRevenue = bookingRevenue,
                    ServiceRevenue = serviceRevenue,
                    GrowthRate = growthRate,
                    TopSports = topSports,
                    MonthlySummaries = monthlySummaries
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy dữ liệu báo cáo từ {FromDate} đến {ToDate}", fromDate, toDate);
                throw;
            }
        }

        public async Task<decimal> CalculateBookingRevenueAsync(DateTime fromDate, DateTime toDate)
        {
            // Booking Revenue = Thanh toán Booking - Hoàn tiền đặt sân
            var bookingIncome = await _context.Transactions
                .Where(t => t.TransactionDate >= fromDate
                         && t.TransactionDate <= toDate
                         && t.Status.Trim() != "Chờ xác nhận"
                         && t.TransactionType.Trim() == "Thanh toán Booking")
                .SumAsync(t => t.Amount);

            var bookingRefund = await _context.Transactions
                .Where(t => t.TransactionDate >= fromDate
                         && t.TransactionDate <= toDate
                         && t.Status.Trim() != "Chờ xác nhận"
                         && t.TransactionType.Trim() == "Hoàn tiền đặt sân")
                .SumAsync(t => t.Amount);

            var revenue = bookingIncome - bookingRefund;

            _logger.LogInformation("Booking Revenue từ {FromDate} đến {ToDate}: {Income}₫ - {Refund}₫ = {Revenue}₫",
                fromDate, toDate, bookingIncome, bookingRefund, revenue);

            return revenue;
        }

        public async Task<decimal> CalculateServiceRevenueAsync(DateTime fromDate, DateTime toDate)
        {
            // Service Revenue = Thanh toán Order - Hoàn tiền
            var orderIncome = await _context.Transactions
                .Where(t => t.TransactionDate >= fromDate
                         && t.TransactionDate <= toDate
                         && t.Status.Trim() != "Chờ xác nhận"
                         && t.TransactionType.Trim() == "Thanh toán Order")
                .SumAsync(t => t.Amount);

            var orderRefund = await _context.Transactions
                .Where(t => t.TransactionDate >= fromDate
                         && t.TransactionDate <= toDate
                         && t.Status.Trim() != "Chờ xác nhận"
                         && t.TransactionType.Trim() == "Hoàn tiền")
                .SumAsync(t => t.Amount);

            var revenue = orderIncome - orderRefund;

            _logger.LogInformation("Service Revenue từ {FromDate} đến {ToDate}: {Income}₫ - {Refund}₫ = {Revenue}₫",
                fromDate, toDate, orderIncome, orderRefund, revenue);

            return revenue;
        }

        public async Task<List<SportStatsViewModel>> GetTopSportsAsync(DateTime fromDate, DateTime toDate)
        {
            // Lấy doanh thu booking theo từng môn thể thao (Thanh toán Booking)
            // Không xét status
            var sportStats = await _context.Transactions
                .Include(t => t.Booking)
                    .ThenInclude(b => b.Pitch)
                    .ThenInclude(p => p.Category)
                .Where(t => t.TransactionDate >= fromDate
                         && t.TransactionDate <= toDate
                         && t.Status.Trim() != "Chờ xác nhận"
                         && t.TransactionType.Trim() == "Thanh toán Booking"
                         && t.Booking != null
                         && t.Booking.Pitch != null
                         && t.Booking.Pitch.Category != null
                         && t.Booking.Pitch.Category.Type == "Pitch")
                .GroupBy(t => t.Booking.Pitch.Category.CategoryName)
                .Select(g => new
                {
                    SportName = g.Key,
                    Revenue = g.Sum(x => x.Amount),
                    BookingCount = g.Count()
                })
                .OrderByDescending(x => x.Revenue)
                .Take(3)
                .ToListAsync();

            // Trừ hoàn tiền cho từng môn thể thao
            var results = new List<SportStatsViewModel>();
            var previousPeriodDays = (toDate - fromDate).Days;
            var previousFromDate = fromDate.AddDays(-previousPeriodDays);
            var previousToDate = fromDate.AddTicks(-1);

            foreach (var stat in sportStats)
            {
                // Trừ hoàn tiền đặt sân cho môn thể thao này (không xét status)
                var refund = await _context.Transactions
                    .Include(t => t.Booking)
                        .ThenInclude(b => b.Pitch)
                        .ThenInclude(p => p.Category)
                    .Where(t => t.TransactionDate >= fromDate
                             && t.TransactionDate <= toDate
                             && t.Status.Trim() != "Chờ xác nhận"
                             && t.TransactionType.Trim() == "Hoàn tiền đặt sân"
                             && t.Booking != null
                             && t.Booking.Pitch != null
                             && t.Booking.Pitch.Category != null
                             && t.Booking.Pitch.Category.CategoryName == stat.SportName)
                    .SumAsync(x => x.Amount);

                var actualRevenue = stat.Revenue - refund;

                // Tính doanh thu kỳ trước (không xét status)
                var previousIncome = await _context.Transactions
                    .Include(t => t.Booking)
                        .ThenInclude(b => b.Pitch)
                        .ThenInclude(p => p.Category)
                    .Where(t => t.TransactionDate >= previousFromDate
                             && t.TransactionDate <= previousToDate
                             && t.Status.Trim() != "Chờ xác nhận"
                             && t.TransactionType.Trim() == "Thanh toán Booking"
                             && t.Booking != null
                             && t.Booking.Pitch != null
                             && t.Booking.Pitch.Category != null
                             && t.Booking.Pitch.Category.CategoryName == stat.SportName)
                    .SumAsync(x => x.Amount);

                var previousRefund = await _context.Transactions
                    .Include(t => t.Booking)
                        .ThenInclude(b => b.Pitch)
                        .ThenInclude(p => p.Category)
                    .Where(t => t.TransactionDate >= previousFromDate
                             && t.TransactionDate <= previousToDate
                             && t.TransactionType.Trim() == "Hoàn tiền đặt sân"
                             && t.Booking != null
                             && t.Booking.Pitch != null
                             && t.Booking.Pitch.Category != null
                             && t.Booking.Pitch.Category.CategoryName == stat.SportName)
                    .SumAsync(x => x.Amount);

                var previousRevenue = previousIncome - previousRefund;

                double growth = 0;
                if (previousRevenue > 0)
                {
                    growth = (double)((actualRevenue - previousRevenue) / previousRevenue * 100);
                }
                else if (actualRevenue > 0)
                {
                    growth = 100; // Tăng trưởng 100% nếu kỳ trước không có doanh thu
                }

                var viewModel = new SportStatsViewModel
                {
                    SportName = stat.SportName,
                    Revenue = actualRevenue,
                    BookingCount = stat.BookingCount,
                    Growth = Math.Round(growth, 1)
                };

                // Gán icon và màu dựa trên tên môn thể thao
                AssignSportStyle(viewModel);
                results.Add(viewModel);
            }

            return results;
        }

        public async Task<List<MonthlySummaryViewModel>> GetMonthlySummariesAsync(int monthCount = 5)
        {
            var summaries = new List<MonthlySummaryViewModel>();
            var currentDate = DateTime.Now;

            for (int i = 0; i < monthCount; i++)
            {
                var month = currentDate.AddMonths(-i);
                DateTime fromDate, toDate;

                // Nếu là tháng hiện tại, lấy từ đầu tháng đến hiện tại
                if (i == 0)
                {
                    fromDate = new DateTime(month.Year, month.Month, 1);
                    toDate = currentDate;
                }
                else
                {
                    fromDate = new DateTime(month.Year, month.Month, 1);
                    toDate = fromDate.AddMonths(1).AddTicks(-1);
                }

                var bookingRevenue = await CalculateBookingRevenueAsync(fromDate, toDate);
                var serviceRevenue = await CalculateServiceRevenueAsync(fromDate, toDate);
                var totalRevenue = bookingRevenue + serviceRevenue;

                // Đếm số giao dịch (chỉ tính Booking và Order, không tính refund, không xét status)
                var transactionCount = await _context.Transactions
                    .Where(t => t.TransactionDate >= fromDate
                             && t.TransactionDate <= toDate
                             && t.Status.Trim() != "Chờ xác nhận"
                             && (t.TransactionType.Trim() == "Thanh toán Booking"
                              || t.TransactionType.Trim() == "Thanh toán Order"))
                    .CountAsync();

                var averagePerTransaction = transactionCount > 0
                    ? totalRevenue / transactionCount
                    : 0;

                // Tính growth so với tháng trước
                double growthRate = 0;
                if (i < monthCount - 1)
                {
                    var previousMonth = currentDate.AddMonths(-(i + 1));
                    var previousFromDate = new DateTime(previousMonth.Year, previousMonth.Month, 1);
                    var previousToDate = previousFromDate.AddMonths(1).AddTicks(-1);

                    var previousBookingRevenue = await CalculateBookingRevenueAsync(previousFromDate, previousToDate);
                    var previousServiceRevenue = await CalculateServiceRevenueAsync(previousFromDate, previousToDate);
                    var previousTotal = previousBookingRevenue + previousServiceRevenue;

                    if (previousTotal > 0)
                    {
                        growthRate = (double)((totalRevenue - previousTotal) / previousTotal * 100);
                    }
                    else if (totalRevenue > 0)
                    {
                        growthRate = 100;
                    }
                }

                var isCurrentMonth = i == 0;

                var summary = new MonthlySummaryViewModel
                {
                    MonthLabel = $"Tháng {month.Month}/{month.Year}",
                    DateRange = isCurrentMonth
                        ? $"{fromDate:dd/MM} - {currentDate:dd/MM}"
                        : "Toàn tháng",
                    TotalRevenue = totalRevenue,
                    TransactionCount = transactionCount,
                    AveragePerTransaction = averagePerTransaction,
                    GrowthRate = Math.Round(growthRate, 1)
                };

                summaries.Add(summary);
            }

            return summaries;
        }

        public async Task<List<MonthlySummaryViewModel>> GetMonthlySummariesByRangeAsync(DateTime startMonth, DateTime endMonth)
        {
            var summaries = new List<MonthlySummaryViewModel>();
            var now = DateTime.Now;

            // Normalize dates to first day of month
            var currentMonth = new DateTime(startMonth.Year, startMonth.Month, 1);
            var lastMonth = new DateTime(endMonth.Year, endMonth.Month, 1);

            while (currentMonth <= lastMonth)
            {
                var fromDate = currentMonth;
                var toDate = currentMonth.AddMonths(1).AddTicks(-1);

                // Nếu là tháng hiện tại và chưa kết thúc, chỉ lấy đến thời điểm hiện tại
                if (currentMonth.Year == now.Year && currentMonth.Month == now.Month)
                {
                    toDate = now;
                }

                var bookingRevenue = await CalculateBookingRevenueAsync(fromDate, toDate);
                var serviceRevenue = await CalculateServiceRevenueAsync(fromDate, toDate);
                var totalRevenue = bookingRevenue + serviceRevenue;

                // Đếm số giao dịch (chỉ tính Booking và Order, không tính refund, không xét status)
                var transactionCount = await _context.Transactions
                    .Where(t => t.TransactionDate >= fromDate
                             && t.TransactionDate <= toDate
                             && t.Status.Trim() != "Chờ xác nhận"
                             && (t.TransactionType.Trim() == "Thanh toán Booking"
                              || t.TransactionType.Trim() == "Thanh toán Order"))
                    .CountAsync();

                var averagePerTransaction = transactionCount > 0
                    ? totalRevenue / transactionCount
                    : 0;

                // Tính growth so với tháng trước
                double growthRate = 0;
                var previousMonth = currentMonth.AddMonths(-1);

                if (previousMonth >= new DateTime(2020, 1, 1)) // Giới hạn không tính quá xa
                {
                    var previousFromDate = new DateTime(previousMonth.Year, previousMonth.Month, 1);
                    var previousToDate = previousFromDate.AddMonths(1).AddTicks(-1);

                    var previousBookingRevenue = await CalculateBookingRevenueAsync(previousFromDate, previousToDate);
                    var previousServiceRevenue = await CalculateServiceRevenueAsync(previousFromDate, previousToDate);
                    var previousTotal = previousBookingRevenue + previousServiceRevenue;

                    if (previousTotal > 0)
                    {
                        growthRate = (double)((totalRevenue - previousTotal) / previousTotal * 100);
                    }
                    else if (totalRevenue > 0)
                    {
                        growthRate = 100;
                    }
                }

                var isCurrentMonth = currentMonth.Year == now.Year && currentMonth.Month == now.Month;

                var summary = new MonthlySummaryViewModel
                {
                    MonthLabel = $"Tháng {currentMonth.Month}/{currentMonth.Year}",
                    DateRange = isCurrentMonth
                        ? $"{fromDate:dd/MM} - {toDate:dd/MM}"
                        : "Toàn tháng",
                    TotalRevenue = totalRevenue,
                    TransactionCount = transactionCount,
                    AveragePerTransaction = averagePerTransaction,
                    GrowthRate = Math.Round(growthRate, 1)
                };

                summaries.Add(summary);
                currentMonth = currentMonth.AddMonths(1);
            }

            return summaries.OrderByDescending(s => s.MonthLabel).ToList();
        }

        public async Task<double> CalculateGrowthRateAsync(DateTime fromDate, DateTime toDate)
        {
            var currentRevenue = await CalculateBookingRevenueAsync(fromDate, toDate)
                               + await CalculateServiceRevenueAsync(fromDate, toDate);

            // Tính doanh thu kỳ trước (cùng khoảng thời gian)
            var periodDays = (toDate - fromDate).Days;
            var previousFromDate = fromDate.AddDays(-periodDays - 1);
            var previousToDate = fromDate.AddTicks(-1);

            var previousRevenue = await CalculateBookingRevenueAsync(previousFromDate, previousToDate)
                                + await CalculateServiceRevenueAsync(previousFromDate, previousToDate);

            if (previousRevenue == 0)
                return currentRevenue > 0 ? 100 : 0;

            var growth = (double)((currentRevenue - previousRevenue) / previousRevenue * 100);
            return Math.Round(growth, 1);
        }

        public async Task<ChartDataViewModel> GetChartDataAsync(DateTime fromDate, DateTime toDate)
        {
            var chartData = new ChartDataViewModel();
            var daysDiff = (toDate - fromDate).Days;

            // Nếu khoảng thời gian <= 31 ngày, hiển thị theo ngày
            if (daysDiff <= 31)
            {
                for (var date = fromDate; date <= toDate; date = date.AddDays(1))
                {
                    var nextDay = date.AddDays(1).AddTicks(-1);

                    chartData.Labels.Add(date.ToString("dd/MM"));

                    var bookingRevenue = await CalculateBookingRevenueAsync(date, nextDay);
                    var serviceRevenue = await CalculateServiceRevenueAsync(date, nextDay);

                    chartData.BookingData.Add(bookingRevenue);
                    chartData.ServiceData.Add(serviceRevenue);
                }
            }
            // Nếu > 31 ngày, nhóm theo tuần
            else if (daysDiff <= 90)
            {
                var currentDate = fromDate;
                int weekNumber = 1;

                while (currentDate <= toDate)
                {
                    var weekEnd = currentDate.AddDays(6) > toDate
                        ? toDate
                        : currentDate.AddDays(6);

                    chartData.Labels.Add($"Tuần {weekNumber}");

                    var bookingRevenue = await CalculateBookingRevenueAsync(currentDate, weekEnd);
                    var serviceRevenue = await CalculateServiceRevenueAsync(currentDate, weekEnd);

                    chartData.BookingData.Add(bookingRevenue);
                    chartData.ServiceData.Add(serviceRevenue);

                    currentDate = weekEnd.AddDays(1);
                    weekNumber++;
                }
            }
            // Nếu > 90 ngày, nhóm theo tháng
            else
            {
                var currentMonth = new DateTime(fromDate.Year, fromDate.Month, 1);
                var endMonth = new DateTime(toDate.Year, toDate.Month, 1);

                while (currentMonth <= endMonth)
                {
                    var monthEnd = currentMonth.AddMonths(1).AddTicks(-1);
                    if (monthEnd > toDate) monthEnd = toDate;

                    chartData.Labels.Add(currentMonth.ToString("MM/yyyy"));

                    var bookingRevenue = await CalculateBookingRevenueAsync(currentMonth, monthEnd);
                    var serviceRevenue = await CalculateServiceRevenueAsync(currentMonth, monthEnd);

                    chartData.BookingData.Add(bookingRevenue);
                    chartData.ServiceData.Add(serviceRevenue);

                    currentMonth = currentMonth.AddMonths(1);
                }
            }

            return chartData;
        }

        private void AssignSportStyle(SportStatsViewModel model)
        {
            var sportName = model.SportName.ToLower();

            if (sportName.Contains("bóng đá") || sportName.Contains("football"))
            {
                model.IconClass = "fas fa-futbol";
                model.ColorClass = "football";
            }
            else if (sportName.Contains("tennis"))
            {
                model.IconClass = "fas fa-table-tennis";
                model.ColorClass = "tennis";
            }
            else if (sportName.Contains("cầu lông") || sportName.Contains("badminton"))
            {
                model.IconClass = "fas fa-volleyball-ball";
                model.ColorClass = "badminton";
            }
            else if (sportName.Contains("bóng rổ") || sportName.Contains("basketball"))
            {
                model.IconClass = "fas fa-basketball-ball";
                model.ColorClass = "basketball";
            }
            else if (sportName.Contains("bóng chuyền") || sportName.Contains("volleyball"))
            {
                model.IconClass = "fas fa-volleyball-ball";
                model.ColorClass = "volleyball";
            }
            else
            {
                model.IconClass = "fas fa-running";
                model.ColorClass = "default";
            }
        }
    }
}