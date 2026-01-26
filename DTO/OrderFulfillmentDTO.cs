namespace SportBookingSystem.DTO
{
    public class OrderFulfillmentDTO
    {
        public int OrderId { get; set; }
        public string? OrderCode { get; set; }
        public string? CustomerName { get; set; }
        public string? ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal TotalAmount { get; set; }
        public string? Status { get; set; }
        public DateTime OrderDate { get; set; }
    }

    public class FulfillmentRequestDTO
    {
        public int OrderId { get; set; }
        public string NewStatus { get; set; } = string.Empty; 
    }
}
