using SportBookingSystem.Models.ViewModels;

namespace SportBookingSystem.Services
{
    public interface IReportService
    {
        Task<ReportDataViewModel> GetReportDataAsync(DateTime fromDate, DateTime toDate);

        /// Tính doanh thu đặt sân
        Task<decimal> CalculateBookingRevenueAsync(DateTime fromDate, DateTime toDate);

        /// Tính doanh thu dịch vụ/sản phẩm
        Task<decimal> CalculateServiceRevenueAsync(DateTime fromDate, DateTime toDate);

        /// Thống kê theo môn thể thao
        Task<List<SportStatsViewModel>> GetTopSportsAsync(DateTime fromDate, DateTime toDate);

        /// Lấy dữ liệu tổng kết theo tháng
        Task<List<MonthlySummaryViewModel>> GetMonthlySummariesAsync(int monthCount = 5);

        /// Lấy dữ liệu tổng kết theo khoảng tháng cụ thể
        Task<List<MonthlySummaryViewModel>> GetMonthlySummariesByRangeAsync(DateTime startMonth, DateTime endMonth);

        /// Tính tỷ lệ tăng trưởng so với kỳ trước
        Task<double> CalculateGrowthRateAsync(DateTime fromDate, DateTime toDate);

        /// Lấy dữ liệu cho biểu đồ theo thời gian
        Task<ChartDataViewModel> GetChartDataAsync(DateTime fromDate, DateTime toDate);
    }
}