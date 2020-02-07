using DAL;

namespace DTO
{
    public class UserDTO
    {
        public string? UserName { get; set; }

        public int? UserId { get; set; }
        
        public bool? IsOnline { get; set; }
        
        public string? ImageUrl { get; set; }

        public NotificationSettings? NotificationSettings { get; set; }
    }
}