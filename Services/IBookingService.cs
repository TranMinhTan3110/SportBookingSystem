using SportBookingSystem.Models.ViewModels;

namespace SportBookingSystem.Services
{
    public interface IBookingService
    {
        Task<FilterPitchesResponse> GetAvailablePitchesAsync(
            DateTime date,
            int? slotId,
            List<int>? categoryIds,
            decimal? minPrice,
            decimal? maxPrice);

        Task<FilterPitchesResponse> GetFilteredPitchesAsync(
            DateTime date,
            int? slotId,
            List<int>? categoryIds,
            decimal? minPrice,
            decimal? maxPrice,
            List<string>? statusFilter);
    }
}