using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SportBookingSystem.Models.Entities
{
    public class Pitches
    {
        [Key]
        public int PitchId { get; set; }

        [Required]
        [StringLength(100)]
        public string PitchName { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal PricePerHour { get; set; }

        public int Capacity { get; set; } = 5; // 5, 7, 11 người

        public string? ImageUrl { get; set; }

        public string? Description { get; set; }

        [StringLength(50)]
        public string Status { get; set; } = "Sẵn sàng";

        // Khóa ngoại Category
        public int CategoryId { get; set; }
        [ForeignKey("CategoryId")]
        public Categories Category { get; set; }

        public ICollection<Bookings> Bookings { get; set; }
    }
}
