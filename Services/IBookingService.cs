using SportBookingSystem.Models.ViewModels;

namespace SportBookingSystem.Services
{
    public interface IBookingService
    {
        // Các hàm lọc sân cũ (giữ nguyên)
        Task<FilterPitchesResponse> GetAvailablePitchesAsync(
            DateTime date,
            List<int>? slotIds,
            List<int>? categoryIds,
            List<int>? specificPitchIds,
            List<int>? capacities);

        Task<FilterPitchesResponse> GetFilteredPitchesAsync(
            DateTime date,
            List<int>? slotIds,
            List<int>? categoryIds,
            List<int>? specificPitchIds,
            List<int>? capacities,
            List<string>? statusFilter);

        Task<FilterPitchesResponse> GetFilteredPitchesWithPaginationAsync(
            DateTime date,
            List<int>? slotIds,
            List<int>? categoryIds,
            List<int>? specificPitchIds,
            List<int>? capacities,
            List<string>? statusFilter,
            int page,
            int pageSize);
        Task<(bool Success, string Message, object? Data)> CheckInBookingAsync(string bookingCode);
        Task<(bool Success, string Message)> ConfirmCheckInAsync(string bookingCode);

        Task<(bool Success, string Message, string? QrBase64, string? BookingCode)> BookPitchAsync(int userId, int pitchId, int slotId, DateTime date);
    }
}