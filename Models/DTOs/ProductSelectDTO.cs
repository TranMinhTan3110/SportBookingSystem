namespace SportBookingSystem.Models.DTOs
{
    public class ProductSelectDTO
    {
        public int ProductID { get; set; }
        public string? ProductName { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
    }
}
