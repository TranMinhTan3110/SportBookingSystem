using SportBookingSystem.Models.Entities;
using SportBookingSystem.Models.ViewModels;

namespace SportBookingSystem.Services
{
    public interface IPitchService
    {
        Task<List<Pitches>> GetAllPitchesAsync(int? categoryId = null, string? status = null, string? searchTerm = null);

        Task<Pitches?> GetPitchByIdAsync(int pitchId);

        Task<(bool Success, string Message, Pitches? Pitch)> CreatePitchAsync(PitchViewModel model);

        Task<(bool Success, string Message)> UpdatePitchAsync(PitchViewModel model);

        Task<(bool Success, string Message)> DeletePitchAsync(int pitchId);

        Task<List<Categories>> GetPitchCategoriesAsync();

        //hàm mới
        Task<List<PitchPriceSetting>> GetPitchPricesAsync(int pitchId);
        Task<(bool Success, string Message)> AddPitchPriceAsync(PitchPriceViewModel model);
        Task<(bool Success, string Message)> DeletePitchPriceAsync(int id);
        
        }
}