using SportBookingSystem.Models.ViewModels;

namespace SportBookingSystem.Services
{
    public interface IReportService
    {
        /// <summary>
        /// Lấy dữ liệu báo cáo theo khoảng thời gian
        /// </summary>
        Task<ReportDataViewModel> GetReportDataAsync(DateTime fromDate, DateTime toDate);

        /// <summary>
        /// Tính doanh thu đặt sân
        /// </summary>
        Task<decimal> CalculateBookingRevenueAsync(DateTime fromDate, DateTime toDate);

        /// <summary>
        /// Tính doanh thu dịch vụ/sản phẩm
        /// </summary>
        Task<decimal> CalculateServiceRevenueAsync(DateTime fromDate, DateTime toDate);

        /// <summary>
        /// Thống kê theo môn thể thao
        /// </summary>
        Task<List<SportStatsViewModel>> GetTopSportsAsync(DateTime fromDate, DateTime toDate);

        /// <summary>
        /// Lấy dữ liệu tổng kết theo tháng
        /// </summary>
        Task<List<MonthlySummaryViewModel>> GetMonthlySummariesAsync(int monthCount = 5);

        /// <summary>
        /// Lấy dữ liệu tổng kết theo khoảng tháng cụ thể
        /// </summary>
        Task<List<MonthlySummaryViewModel>> GetMonthlySummariesByRangeAsync(DateTime startMonth, DateTime endMonth);

        /// <summary>
        /// Tính tỷ lệ tăng trưởng so với kỳ trước
        /// </summary>
        Task<double> CalculateGrowthRateAsync(DateTime fromDate, DateTime toDate);

        /// <summary>
        /// Lấy dữ liệu cho biểu đồ theo thời gian
        /// </summary>
        Task<ChartDataViewModel> GetChartDataAsync(DateTime fromDate, DateTime toDate);
    }
}