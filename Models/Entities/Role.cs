using System.ComponentModel.DataAnnotations;

namespace SportBookingSystem.Models.Entities
{
    public class Role
    {
        [Key]
        public int RoleId { get; set; }

        [Required]
        [StringLength(50)]
        public string RoleName { get; set; }

      
        public ICollection<Users> User { get; set; }
    }
}
