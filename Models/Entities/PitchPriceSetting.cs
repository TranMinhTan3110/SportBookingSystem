using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SportBookingSystem.Models.Entities
{
    public class PitchPriceSetting
    {
        [Key]
        public int Id { get; set; }

        public int PitchId { get; set; }
        [ForeignKey("PitchId")]
        public Pitches Pitch { get; set; }

        [Required]
        public TimeSpan StartTime { get; set; } // Ví dụ: 17:00

        [Required]
        public TimeSpan EndTime { get; set; }   // Ví dụ: 21:00

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Price { get; set; }      // Ví dụ: 300,000

        public string? Note { get; set; }       // Ví dụ: "Giờ vàng"
    }
}