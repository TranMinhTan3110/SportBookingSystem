namespace SportBookingSystem.DTO
{
    public class UserInfo
    {
        public string Id { get; set; }
        public string FullName { get; set; }
   public string PhoneNumber { get; set; }
        public string Email { get; set; }
        
        public string Role { get; set; } 
        public string CreatedAt { get; set; }
        public bool IsActive { get; set; } 

    }
}
