namespace SportBookingSystem.Models.DTOs
{
    public class CreateDepositDTO
    {
        public int UserID { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string? Message { get; set; }
    }
}
