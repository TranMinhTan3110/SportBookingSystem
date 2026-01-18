namespace SportBookingSystem.Models.ViewModels
{
    public class PitchSlotViewModel
    {
        public int PitchId { get; set; }
        public string PitchName { get; set; }
        public string CategoryName { get; set; }
        public int Capacity { get; set; }
        public string ImageUrl { get; set; }
        public string Description { get; set; }

        // Thông tin slot
        public int SlotId { get; set; }
        public string SlotName { get; set; }
        public string TimeRange { get; set; }

        // Giá cả
        public decimal PricePerHour { get; set; }
        public decimal FullPrice { get; set; }
        public decimal DepositPrice { get; set; }

        // Trạng thái
        public string Status { get; set; } // "available", "limited", "booked"
        public string StatusText { get; set; } // "Còn trống", "Sắp hết", "Đã đặt"
        public bool IsAvailable { get; set; }
    }

    public class FilterPitchesRequest
    {
        public DateTime Date { get; set; } = DateTime.Today;
        public int? SlotId { get; set; }
        public List<int>? CategoryIds { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public List<string>? StatusFilter { get; set; } // "available", "booked", "maintenance"
    }

    public class FilterPitchesResponse
    {
        public List<PitchSlotViewModel> PitchSlots { get; set; }
        public int TotalCount { get; set; }
        public int DisplayCount { get; set; }
    }
}