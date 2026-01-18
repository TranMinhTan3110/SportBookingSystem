using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SportBookingSystem.Models.Entities
{
    public class Transactions
    {
        [Key]
        public int TransactionId { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Amount { get; set; }

        [StringLength(255)]
        public string? Message { get; set; }

        [StringLength(50)]
        public string? TransactionType { get; set; }

        public DateTime TransactionDate { get; set; } = DateTime.Now;

       
        public int UserId { get; set; }
        [ForeignKey("UserId")]
        [InverseProperty("SentTransactions")]
        public Users Sender { get; set; }

        public int? ReceiverId { get; set; }
        [ForeignKey("ReceiverId")]
        [InverseProperty("ReceivedTransactions")] 
        public Users? Receiver { get; set; }
    }
}

