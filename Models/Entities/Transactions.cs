using SportBookingSystem.Models.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Transactions
{
    [Key]
    public int TransactionId { get; set; }

    [StringLength(20)]
    public string? TransactionCode { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal Amount { get; set; }

    [StringLength(255)]
    public string? Message { get; set; }

    [StringLength(50)]
    public string? TransactionType { get; set; }

    [StringLength(50)]
    public string? Source { get; set; }

    [StringLength(20)]
    public string Status { get; set; } = "Chờ xử lý";

    public DateTime TransactionDate { get; set; } = DateTime.Now;

    [Column(TypeName = "decimal(18, 2)")]
    public decimal? BalanceAfter { get; set; }

    // Đổi SenderId quay lại thành UserId để khớp với HomeController, LoginController...
    public int UserId { get; set; }
    [ForeignKey("UserId")]
    [InverseProperty("SentTransactions")]
    public Users Sender { get; set; }

    public int? ReceiverId { get; set; }
    [ForeignKey("ReceiverId")]
    [InverseProperty("ReceivedTransactions")]
    public Users? Receiver { get; set; }

    public int? BookingId { get; set; }
    [ForeignKey("BookingId")]
    public Bookings? Booking { get; set; }

    public int? OrderId { get; set; }
    [ForeignKey("OrderId")]
    public Orders? Order { get; set; }
}