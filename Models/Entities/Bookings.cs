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
        public int SlotId { get; set; }
        [ForeignKey("SlotId")]
        public TimeSlots TimeSlot { get; set; }
        public bool IsPaid { get; set; } = false;

        [StringLength(255)]
        public string? Note { get; set; }

        public int Status { get; set; } = 0; 

        [StringLength(50)]
        public string? CheckInCode { get; set; }

        // Khóa ngoại
        public int? UserId { get; set; } 
        [ForeignKey("UserId")]
        public Users? User { get; set; }

        public int PitchId { get; set; }
        [ForeignKey("PitchId")]
        public Pitches Pitch { get; set; }

        public ICollection<BookingServices> BookingServices { get; set; }
    }
}
