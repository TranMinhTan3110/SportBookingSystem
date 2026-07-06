using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace SportBookingSystem.Services
{
    public interface ICloudinaryService
    {
        Task<string?> UploadImageAsync(IFormFile file, string folderName);

        Task<bool> DeleteImageAsync(string imageUrl);
    }
}
