using System.ComponentModel.DataAnnotations;

namespace SportBookingSystem.Models.Entities
{
    public class SystemSetting
    {
        [Key] 
        [MaxLength(100)]
        public string SettingKey { get; set; } = string.Empty;

        [Required]
        public string SettingValue { get; set; } = string.Empty;
    }
}