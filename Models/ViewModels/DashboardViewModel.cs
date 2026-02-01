namespace SportBookingSystem.Models.ViewModels
{
    public class DashboardViewModel
    {
        // Stats Cards
        public decimal TodayRevenue { get; set; }
        public decimal RevenueGrowthPercent { get; set; }
        public bool IsRevenueGrowthPositive { get; set; }

        public int TodayBookingsCount { get; set; }
        public decimal BookingGrowthPercent { get; set; }
        public bool IsBookingGrowthPositive { get; set; }

        public int ActivePitchesCount { get; set; }
        public int TotalPitchesCount { get; set; }

        // Revenue Chart Data (7 days)
        public List<RevenueChartData> RevenueChart { get; set; } = new();

        // Top Customers by Bookings
        public List<TopCustomerData> TopCustomers { get; set; } = new();

        // Today's Bookings Schedule
        public List<TodayBookingData> TodayBookings { get; set; } = new();

        // Top Pitches by Bookings
        public List<TopPitchData> TopPitches { get; set; } = new();
    }

    public class RevenueChartData
    {
        public string Date { get; set; } // Format: "dd/MM"
        public string DayOfWeek { get; set; } // T2, T3, T4, T5, T6, T7, CN
        public decimal Revenue { get; set; }
    }

    public class TopCustomerData
    {
        public string CustomerName { get; set; }
        public int BookingCount { get; set; }
        public decimal TotalAmount { get; set; }
    }

    public class TodayBookingData
    {
        public int BookingId { get; set; }
        public string StartTime { get; set; } // Format: "HH:mm"
        public string PitchName { get; set; }
        public string CustomerName { get; set; }
        public string StatusText { get; set; }
        public string StatusClass { get; set; } // confirmed, pending, completed, cancelled
    }

    public class TopPitchData
    {
        public int Rank { get; set; }
        public string PitchName { get; set; }
        public int BookingCount { get; set; }
        public decimal TotalRevenue { get; set; }
    }
}