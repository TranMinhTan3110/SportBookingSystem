using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SportBookingSystem.Models.Entities
{
    public class OrderDetails
    {
        [Key]
        public int OrderDetailId { get; set; }

        public int Quantity { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Price { get; set; }

        // Khóa ngoại
        public int OrderId { get; set; }
        [ForeignKey("OrderId")]
        public Orders Order { get; set; }

        public int ProductId { get; set; }
        [ForeignKey("ProductId")]
        public Products Product { get; set; }
    }
}
