using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SportBookingSystem.Models.Entities
{
    public class PitchSlots
    {
        [Key]
        public int PitchSlotId { get; set; }

        [Required]
        public DateTime PlayDate { get; set; }

        // Liên kết tới sân
        public int PitchId { get; set; }
        [ForeignKey("PitchId")]
        public Pitches Pitch { get; set; }

        // Liên kết tới khung giờ
        public int SlotId { get; set; }
        [ForeignKey("SlotId")]
        public TimeSlots TimeSlot { get; set; }

        // Trạng thái: 0: Trống, 1: Đã đặt, 2: Đang giữ chỗ
        public int Status { get; set; } = 0;

        // Cho phép null: Khi có người đặt thì gắn ID hóa đơn vào đây
        public int? BookingId { get; set; }
        [ForeignKey("BookingId")]
        public Bookings? Booking { get; set; }
    }
}