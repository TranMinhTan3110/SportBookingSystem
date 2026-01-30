using SportBookingSystem.Models.Entities;

namespace SportBookingSystem.Models.ViewModels
{
    public class HomeViewModel
    {
        public Users User { get; set; }
        public int UpcomingBookingsCount { get; set; }
        public int PendingBookingsCount { get; set; }
        public List<Transactions> RecentTransactions { get; set; }
        public List<Pitches> FeaturedPitches { get; set; }
        public List<Products> FeaturedFoodItems { get; set; }
        public List<Products> FeaturedSupplies { get; set; }
    }
}
