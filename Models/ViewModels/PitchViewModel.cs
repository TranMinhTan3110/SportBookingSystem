using System.ComponentModel.DataAnnotations;

namespace SportBookingSystem.Models.ViewModels
{
    public class PitchViewModel
    {
        public int PitchId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên sân")]
        [StringLength(100, ErrorMessage = "Tên sân không được vượt quá 100 ký tự")]
        public string PitchName { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập giá thuê")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá thuê phải lớn hơn 0")]
        public decimal PricePerHour { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập sức chứa")]
        [Range(1, 100, ErrorMessage = "Sức chứa phải từ 1-100 người")]
        public int Capacity { get; set; }

        public string? Description { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn trạng thái")]
        public string Status { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn loại sân")]
        public int CategoryId { get; set; }

        // Để nhận file ảnh từ form
        public IFormFile? ImageFile { get; set; }

        // URL ảnh hiện tại (dùng cho Edit)
        public string? ImageUrl { get; set; }
    }
}