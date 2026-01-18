using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SportBookingSystem.Models.Entities
{
    public class BookingServices
    {
        [Key]
        public int BookingServiceId { get; set; }

        public int Quantity { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Price { get; set; } // Giá tại thời điểm đặt

        // Khóa ngoại
        public int BookingId { get; set; }
        [ForeignKey("BookingId")]
        public Bookings Booking { get; set; }

        public int ProductId { get; set; }
        [ForeignKey("ProductId")]
        public Products Product { get; set; }
    }
}
