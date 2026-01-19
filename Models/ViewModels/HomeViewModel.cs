using SportBookingSystem.Models.Entities;

namespace SportBookingSystem.Models.ViewModels
{
    public class HomeViewModel
    {
        public Users User { get; set; }
        public int UpcomingBookingsCount { get; set; }
        public List<Transactions> RecentTransactions { get; set; }
    }
}
