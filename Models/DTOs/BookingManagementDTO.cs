namespace SportBookingSystem.Models.DTOs
{
    public class BookingManagementDTO
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string CustomerName { get; set; }
        public string CustomerPhone { get; set; }
        public string PitchName { get; set; }
        public string Date { get; set; }
        public string Time { get; set; }
        public int SlotId { get; set; }
        public decimal TotalPrice { get; set; }
    }
}
