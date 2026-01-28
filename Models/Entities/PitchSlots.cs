using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SportBookingSystem.Models.Entities
{
    public class PitchSlots
    {
        [Key]
        public int PitchSlotId { get; set; }

        [Required]
        public DateTime PlayDate { get; set; }

        public int PitchId { get; set; }
        [ForeignKey("PitchId")]
        public virtual Pitches Pitch { get; set; }

        public int SlotId { get; set; }
        [ForeignKey("SlotId")]
        public virtual TimeSlots TimeSlot { get; set; }

        public string? BookingCode { get; set; }

    
        public int Status { get; set; } = 0;

        public int? BookingId { get; set; }
        [ForeignKey("BookingId")]
        public virtual Bookings? Booking { get; set; }

    

        
        public int? UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual Users? User { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}