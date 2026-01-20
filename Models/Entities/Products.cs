using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SportBookingSystem.Models.Entities
{
    public class Products
    {
        [Key]
        public int ProductId { get; set; }

        [Required]
        [StringLength(100)]
        public string ProductName { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Price { get; set; }

        [StringLength(20)]
        public string? Unit { get; set; }

        public int StockQuantity { get; set; } = 0;

        public string? ImageUrl { get; set; }

        [StringLength(20)]
        public string? Size { get; set; } // S, M, L, 40, 41...

        [StringLength(50)]
        public string? Brand { get; set; } // Nike, Adidas...

        [StringLength(50)]
        public string? ProductType { get; set; }

        // Khóa ngoại Category
        public int CategoryId { get; set; }
        [ForeignKey("CategoryId")]
        public Categories? Category { get; set; }

        public bool Status { get; set; } = true; // true: Hiển thị, false: Ẩn
    }
}
