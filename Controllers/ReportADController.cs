using Microsoft.AspNetCore.Mvc;
using SportBookingSystem.Services;

namespace SportBookingSystem.Controllers
{
    public class ReportADController : Controller
    {
        private readonly IReportService _reportService;
        private readonly ILogger<ReportADController> _logger;

        public ReportADController(IReportService reportService, ILogger<ReportADController> logger)
        {
            _reportService = reportService;
            _logger = logger;
        }

        /// Hiển thị trang báo cáo với dữ liệu 30 ngày gần nhất
        public async Task<IActionResult> Index()
        {
            try
            {
                // Mặc định 30 ngày gần nhất
                var toDate = DateTime.Now;
                var fromDate = toDate.AddDays(-30);

                var reportData = await _reportService.GetReportDataAsync(fromDate, toDate);

                ViewBag.FromDate = fromDate.ToString("yyyy-MM-dd");
                ViewBag.ToDate = toDate.ToString("yyyy-MM-dd");

                return View(reportData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải trang báo cáo");
                TempData["Error"] = "Có lỗi xảy ra khi tải dữ liệu báo cáo. Vui lòng thử lại.";
                return View();
            }
        }

        /// API lấy dữ liệu báo cáo theo khoảng thời gian
        [HttpGet]
        public async Task<JsonResult> GetReportData(DateTime fromDate, DateTime toDate)
        {
            try
            {
                // Validate dates
                if (fromDate > toDate)
                {
                    return Json(new { success = false, message = "Ngày bắt đầu không được lớn hơn ngày kết thúc" });
                }

                if (toDate > DateTime.Now)
                {
                    toDate = DateTime.Now;
                }

                var reportData = await _reportService.GetReportDataAsync(fromDate, toDate);

                return Json(new
                {
                    success = true,
                    totalRevenue = reportData.TotalRevenue,
                    bookingRevenue = reportData.BookingRevenue,
                    serviceRevenue = reportData.ServiceRevenue,
                    growthRate = reportData.GrowthRate,
                    topSports = reportData.TopSports,
                    monthlySummaries = reportData.MonthlySummaries
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy dữ liệu báo cáo từ {FromDate} đến {ToDate}", fromDate, toDate);
                return Json(new { success = false, message = "Có lỗi xảy ra khi lấy dữ liệu" });
            }
        }

        /// API lấy dữ liệu cho biểu đồ
        [HttpGet]
        public async Task<JsonResult> GetChartData(DateTime fromDate, DateTime toDate)
        {
            try
            {
                var chartData = await _reportService.GetChartDataAsync(fromDate, toDate);

                return Json(new
                {
                    success = true,
                    labels = chartData.Labels,
                    bookingData = chartData.BookingData,
                    serviceData = chartData.ServiceData
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy dữ liệu biểu đồ");
                return Json(new { success = false, message = "Có lỗi xảy ra khi lấy dữ liệu biểu đồ" });
            }
        }

        /// API lấy dữ liệu tổng kết theo tháng với bộ lọc
        [HttpGet]
        public async Task<JsonResult> GetMonthlySummaries(int? fromMonth, int? fromYear, int? toMonth, int? toYear)
        {
            try
            {
                // Mặc định 3 tháng gần nhất
                var currentDate = DateTime.Now;

                DateTime startDate, endDate;

                if (fromMonth.HasValue && fromYear.HasValue && toMonth.HasValue && toYear.HasValue)
                {
                    startDate = new DateTime(fromYear.Value, fromMonth.Value, 1);
                    endDate = new DateTime(toYear.Value, toMonth.Value, 1);

                    if (startDate > endDate)
                    {
                        return Json(new { success = false, message = "Tháng bắt đầu không được lớn hơn tháng kết thúc" });
                    }
                }
                else
                {
                    // Mặc định từ 2 tháng trước đến tháng hiện tại
                    endDate = new DateTime(currentDate.Year, currentDate.Month, 1);
                    startDate = endDate.AddMonths(-2);
                }

                var monthCount = ((endDate.Year - startDate.Year) * 12) + endDate.Month - startDate.Month + 1;
                var summaries = await _reportService.GetMonthlySummariesByRangeAsync(startDate, endDate);

                return Json(new
                {
                    success = true,
                    monthlySummaries = summaries
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy tổng kết theo tháng");
                return Json(new { success = false, message = "Có lỗi xảy ra khi lấy dữ liệu" });
            }
        }

        /// Export báo cáo ra Excel (chưa triển khai ở hiện tại)
        [HttpGet]
        public async Task<IActionResult> ExportExcel(DateTime fromDate, DateTime toDate)
        {
            try
            {
                var reportData = await _reportService.GetReportDataAsync(fromDate, toDate);

                TempData["Info"] = "Chức năng xuất Excel đang được phát triển";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xuất báo cáo Excel");
                TempData["Error"] = "Có lỗi xảy ra khi xuất báo cáo";
                return RedirectToAction(nameof(Index));
            }
        }

        /// Lấy báo cáo theo kỳ (tuần, tháng, quý, năm)
        [HttpGet]
        public async Task<JsonResult> GetReportByPeriod(string period)
        {
            try
            {
                DateTime fromDate, toDate = DateTime.Now;

                switch (period?.ToLower())
                {
                    case "today":
                        fromDate = DateTime.Today;
                        break;
                    case "yesterday":
                        fromDate = DateTime.Today.AddDays(-1);
                        toDate = DateTime.Today.AddTicks(-1);
                        break;
                    case "thisweek":
                        fromDate = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek);
                        break;
                    case "lastweek":
                        fromDate = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek - 7);
                        toDate = fromDate.AddDays(6).AddDays(1).AddTicks(-1);
                        break;
                    case "thismonth":
                        fromDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                        break;
                    case "lastmonth":
                        fromDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddMonths(-1);
                        toDate = fromDate.AddMonths(1).AddTicks(-1);
                        break;
                    case "quarter":
                        var currentQuarter = (DateTime.Now.Month - 1) / 3;
                        fromDate = new DateTime(DateTime.Now.Year, currentQuarter * 3 + 1, 1);
                        break;
                    case "year":
                        fromDate = new DateTime(DateTime.Now.Year, 1, 1);
                        break;
                    default:
                        fromDate = DateTime.Now.AddDays(-30);
                        break;
                }

                var reportData = await _reportService.GetReportDataAsync(fromDate, toDate);

                return Json(new
                {
                    success = true,
                    fromDate = fromDate.ToString("yyyy-MM-dd"),
                    toDate = toDate.ToString("yyyy-MM-dd"),
                    data = reportData
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy báo cáo theo kỳ {Period}", period);
                return Json(new { success = false, message = "Có lỗi xảy ra" });
            }
        }
    }
}