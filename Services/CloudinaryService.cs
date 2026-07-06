using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;

namespace SportBookingSystem.Services
{
    public class CloudinaryService : ICloudinaryService
    {
        private readonly Cloudinary _cloudinary;
        private readonly ILogger<CloudinaryService> _logger;

        public CloudinaryService(IConfiguration configuration, ILogger<CloudinaryService> logger)
        {
            _logger = logger;

            var cloudName = configuration["Cloudinary:CloudName"];
            var apiKey = configuration["Cloudinary:ApiKey"];
            var apiSecret = configuration["Cloudinary:ApiSecret"];

            if (string.IsNullOrEmpty(cloudName) || string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(apiSecret) || apiSecret == "YOUR_CLOUDINARY_API_SECRET")
            {
                _logger.LogWarning("Cloudinary configuration is incomplete or contains placeholder value.");
            }

            var account = new Account(cloudName, apiKey, apiSecret);
            _cloudinary = new Cloudinary(account);
            _cloudinary.Api.Secure = true;
        }

        public async Task<string?> UploadImageAsync(IFormFile file, string folderName)
        {
            if (file == null || file.Length == 0) return null;

            try
            {
                using var stream = file.OpenReadStream();
                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(file.FileName, stream),
                    Folder = folderName
                };

                var uploadResult = await _cloudinary.UploadAsync(uploadParams);

                if (uploadResult.Error != null)
                {
                    _logger.LogError($"Cloudinary upload error: {uploadResult.Error.Message}");
                    return null;
                }

                return uploadResult.SecureUrl.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception during Cloudinary upload");
                return null;
            }
        }

        public async Task<bool> DeleteImageAsync(string imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl)) return false;

            try
            {
                var publicId = ExtractPublicIdFromUrl(imageUrl);
                if (string.IsNullOrEmpty(publicId)) return false;

                var deletionParams = new DeletionParams(publicId);
                var deletionResult = await _cloudinary.DestroyAsync(deletionParams);

                if (deletionResult.Result == "ok")
                {
                    return true;
                }

                _logger.LogWarning($"Cloudinary delete failed for public ID {publicId}: {deletionResult.Result}");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Exception during Cloudinary deletion for url: {imageUrl}");
                return false;
            }
        }

        private string? ExtractPublicIdFromUrl(string url)
        {
            if (string.IsNullOrEmpty(url)) return null;

            try
            {
                // URL format: https://res.cloudinary.com/<cloud_name>/image/upload/v<version>/<folder>/<public_id>.<extension>
                var parts = url.Split(new[] { "/image/upload/" }, StringSplitOptions.None);
                if (parts.Length < 2) return null;

                var pathAfterUpload = parts[1]; // v1720261234/pitches/pitch_abc123.jpg

                // Remove the version segment (e.g., v1720261234/) if present
                var firstSlashIdx = pathAfterUpload.IndexOf('/');
                if (firstSlashIdx != -1)
                {
                    var potentialVersion = pathAfterUpload.Substring(0, firstSlashIdx);
                    if (potentialVersion.StartsWith("v") && double.TryParse(potentialVersion.Substring(1), out _))
                    {
                        pathAfterUpload = pathAfterUpload.Substring(firstSlashIdx + 1); // pitches/pitch_abc123.jpg
                    }
                }

                // Remove the file extension
                var lastDotIdx = pathAfterUpload.LastIndexOf('.');
                if (lastDotIdx != -1)
                {
                    pathAfterUpload = pathAfterUpload.Substring(0, lastDotIdx); // pitches/pitch_abc123
                }

                return pathAfterUpload;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error extracting public ID from url: {url}");
                return null;
            }
        }
    }
}
