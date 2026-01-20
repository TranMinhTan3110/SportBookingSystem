namespace SportBookingSystem.Models.DTOs
{
    public class UserSelectDTO
    {
        public int UserID { get; set; }
        public string? Username { get; set; }
        public string? FullName { get; set; }
        public decimal WalletBalance { get; set; }
    }
}
