using System.ComponentModel.DataAnnotations;

namespace SportBookingSystem.Models.Entities
{
    public class TimeSlots
    {
        [Key]
        public int SlotId { get; set; }

        [Required]
        [StringLength(50)]
        public string SlotName { get; set; } 

        [Required]
        public TimeSpan StartTime { get; set; } 

        [Required]
        public TimeSpan EndTime { get; set; }

        public bool IsActive { get; set; } = true;

        // Quan hệ ngược lại với Bookings
        public ICollection<Bookings> Bookings { get; set; }
    }
}