using SportBookingSystem.Models.ViewModels;

namespace SportBookingSystem.Services
{
    public interface IDashboardService
    {
        /// Lấy toàn bộ dữ liệu cho Dashboard
        Task<DashboardViewModel> GetDashboardDataAsync();

        /// Lấy doanh thu hôm nay và tỷ lệ tăng trưởng so với hôm qua
        Task<(decimal todayRevenue, decimal growthPercent, bool isPositive)> GetTodayRevenueAsync();

        /// Lấy số lượt đặt sân hôm nay và tỷ lệ tăng trưởng so với hôm qua
        Task<(int todayCount, decimal growthPercent, bool isPositive)> GetTodayBookingsCountAsync();

        /// Lấy số lượng sân đang hoạt động (không bảo trì)
        Task<(int activeCount, int totalCount)> GetActivePitchesCountAsync();

        /// Lấy dữ liệu doanh thu 7 ngày qua để vẽ biểu đồ
        Task<List<RevenueChartData>> GetLast7DaysRevenueAsync();

        /// Lấy top khách hàng đặt sân nhiều nhất (tháng này)
        Task<List<TopCustomerData>> GetTopCustomersAsync(int topCount = 5);

        /// Lấy lịch đặt sân hôm nay
        Task<List<TodayBookingData>> GetTodayBookingsScheduleAsync();

        /// Lấy top sân được đặt nhiều nhất (tháng này)
        Task<List<TopPitchData>> GetTopPitchesAsync(int topCount = 4);
    }
}