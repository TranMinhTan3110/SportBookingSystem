using SportBookingSystem.Models.Entities;

namespace SportBookingSystem.Models.ViewModels
{
    public class UserProfileViewModel
    {
        public Users User { get; set; }
        public Bookings? UpcomingBooking { get; set; }
        public Orders? LatestOrder { get; set; }
        public List<Orders>? PendingOrders { get; set; }
    }
}
