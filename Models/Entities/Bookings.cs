using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SportBookingSystem.Models.Entities
{
    public class Bookings
    {
        [Key]
        public int BookingId { get; set; }

        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public DateTime BookingDate { get; set; } = DateTime.Now;

        [Column(TypeName = "decimal(18, 2)")]
        public decimal? TotalPrice { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal PaidAmount { get; set; } = 0;

        public bool IsPaid { get; set; } = false;

        [StringLength(255)]
        public string? Note { get; set; }

        public int Status { get; set; } = 0; // 0: Pending, 1: Confirmed, 2: Cancelled

        [StringLength(50)]
        public string? CheckInCode { get; set; }

        // Khóa ngoại
        public int? UserId { get; set; } // Cho phép null nếu khách vãng lai (tùy logic)
        [ForeignKey("UserId")]
        public Users? User { get; set; }

        public int PitchId { get; set; }
        [ForeignKey("PitchId")]
        public Pitches Pitch { get; set; }

        // Quan hệ: Dịch vụ đi kèm
        public ICollection<BookingServices> BookingServices { get; set; }
    }
}
