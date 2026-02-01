using SportBookingSystem.Models.DTOs;
using SportBookingSystem.Models.ViewModels;

namespace SportBookingSystem.Services
{
    public interface IBookingService
    {
        // Các hàm lọc sân cũ
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
        Task<(bool Success, string Message, decimal RefundAmount)> CancelBookingAsync(string bookingCode, bool isAutoCancel = false);

        Task<List<BookingManagementDTO>> GetPendingBookingsAsync(DateTime? fromDate, DateTime? toDate, int? timeSlotId, int? categoryId, string? search);
        Task<(bool Success, string Message)> ApproveBookingAsync(int bookingId);
        Task<(bool Success, string Message)> RejectBookingAsync(int bookingId);
    }
}