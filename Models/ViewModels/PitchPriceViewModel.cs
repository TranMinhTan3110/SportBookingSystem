namespace SportBookingSystem.Models.ViewModels
{
    public class PitchPriceViewModel
    {
        public int Id { get; set; }
        public int PitchId { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public decimal Price { get; set; }
        public string? Note { get; set; }
    }
}