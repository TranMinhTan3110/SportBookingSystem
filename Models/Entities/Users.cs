using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SportBookingSystem.Models.Entities
{
    public class Users
    {
        [Key]
        public int UserId { get; set; }

        [Required]
        [StringLength(50)]
        public string Username { get; set; }

        [Required]
        [StringLength(255)]
        public string Password { get; set; }

        [StringLength(100)]
        public string? FullName { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string? Email { get; set; }

        [Required]
        [StringLength(15)]
        public string Phone { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal WalletBalance { get; set; } = 0;

        public int RewardPoints { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        // Khóa ngoại Role
        public int RoleId { get; set; }
        [ForeignKey("RoleId")]
        public Role Role { get; set; }

        // Các quan hệ
        public ICollection<Bookings> Bookings { get; set; }
        public ICollection<Orders> Orders { get; set; }

        // Quan hệ giao dịch (Gửi và Nhận)
        [InverseProperty("Sender")]
        public ICollection<Transactions> SentTransactions { get; set; }

        [InverseProperty("Receiver")]
        public ICollection<Transactions> ReceivedTransactions { get; set; }
    }
}
