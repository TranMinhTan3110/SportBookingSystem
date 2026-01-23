namespace SportBookingSystem.Models.DTOs
{
    public class CreatePurchaseDTO
    {
        public int UserID { get; set; }
        public int ProductID { get; set; }
        public int Quantity { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
    }
}
