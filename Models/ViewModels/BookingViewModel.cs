using System;
using System.Collections.Generic;

namespace SportBookingSystem.Models.ViewModels
{
    public class PitchGroupViewModel
    {
        public int PitchId { get; set; }
        public string PitchName { get; set; }
        public string CategoryName { get; set; }
        public int Capacity { get; set; }
        public string ImageUrl { get; set; }
        public string Description { get; set; }
        public decimal PricePerHour { get; set; }
        public List<SlotInfoViewModel> Slots { get; set; } = new List<SlotInfoViewModel>();
    }

    public class SlotInfoViewModel
    {
        public int SlotId { get; set; }
        public string SlotName { get; set; }
        public string TimeRange { get; set; }
        public decimal FullPrice { get; set; }
        public decimal DepositPrice { get; set; }
        public string Status { get; set; }
        public string StatusText { get; set; }
        public bool IsAvailable { get; set; }
    }

    public class FilterPitchesRequest
    {
        public DateTime Date { get; set; } = DateTime.Today;
        public List<int>? SlotIds { get; set; }
        public List<int>? SpecificPitchIds { get; set; }
        public List<int>? CategoryIds { get; set; }
        public List<int>? Capacities { get; set; } // Lọc theo sức chứa (10, 14)
        public List<string>? StatusFilter { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class FilterPitchesResponse
    {
        public List<PitchGroupViewModel> Pitches { get; set; }
        public int TotalCount { get; set; }
        public int DisplayCount { get; set; }
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }
    }
}