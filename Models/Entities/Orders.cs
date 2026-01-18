using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SportBookingSystem.Models.Entities
{
    public class Orders
    {
        [Key]
        public int OrderId { get; set; }

        public DateTime OrderDate { get; set; } = DateTime.Now;

        [Column(TypeName = "decimal(18, 2)")]
        public decimal? TotalAmount { get; set; }

        public int Status { get; set; } = 0;

        [StringLength(50)]
        public string? OrderCode { get; set; }

        // Khóa ngoại
        public int? UserId { get; set; }
        [ForeignKey("UserId")]
        public Users? User { get; set; }

        public ICollection<OrderDetails> OrderDetails { get; set; }
    }
}
