namespace SportBookingSystem.DTO
{
    public class RewardSettingDTO
    {
        public decimal AmountStep { get; set; }  
        public int PointBonus { get; set; }      // Số điểm thưởng (1đ)
        public bool IsActive { get; set; }       // Trạng thái hệ thống
    }
}