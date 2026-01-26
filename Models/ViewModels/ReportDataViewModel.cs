namespace SportBookingSystem.Models.ViewModels
{
    public class ReportDataViewModel
    {
        public decimal TotalRevenue { get; set; }
        public decimal BookingRevenue { get; set; }
        public decimal ServiceRevenue { get; set; }
        public double GrowthRate { get; set; }
        public List<SportStatsViewModel> TopSports { get; set; } = new List<SportStatsViewModel>();
        public List<MonthlySummaryViewModel> MonthlySummaries { get; set; } = new List<MonthlySummaryViewModel>();
    }

    public class SportStatsViewModel
    {
        public string SportName { get; set; }
        public decimal Revenue { get; set; }
        public int BookingCount { get; set; }
        public double Growth { get; set; }
        public string IconClass { get; set; } 
        public string ColorClass { get; set; } 
    }

    public class MonthlySummaryViewModel
    {
        public string MonthLabel { get; set; } // "Tháng 1/2025"
        public string DateRange { get; set; } // "01/01 - 17/01"
        public decimal TotalRevenue { get; set; }
        public int TransactionCount { get; set; }
        public decimal AveragePerTransaction { get; set; }
        public double GrowthRate { get; set; }
    }

    public class ChartDataViewModel
    {
        public List<string> Labels { get; set; } = new List<string>();
        public List<decimal> BookingData { get; set; } = new List<decimal>();
        public List<decimal> ServiceData { get; set; } = new List<decimal>();
    }
}