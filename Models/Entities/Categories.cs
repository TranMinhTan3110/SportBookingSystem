using System.ComponentModel.DataAnnotations;

namespace SportBookingSystem.Models.Entities
{
    public class Categories
    {
        [Key]
        public int CategoryId { get; set; }

        [Required]
        [StringLength(100)]
        public string CategoryName { get; set; }

        [StringLength(50)]
        public string? Type { get; set; } // 'Pitch', 'Product', 'Service'

        // Quan hệ
        public ICollection<Pitches> Pitches { get; set; }
        public ICollection<Products> Products { get; set; }
    }
}
