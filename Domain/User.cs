using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DAL
{
    public class User
    {
        public int UserId { get; set; }

        [MinLength(2)]
        [MaxLength(64)]
        public string UserName { get; set; } = default!;

        public Login Login { get; set; } = default!;

        public bool IsOnline { get; set; } = default!;

        public ICollection<ChatUser>? UserChats { get; set; }

        [MinLength(5)]
        [MaxLength(128)]
        public string? ImageUrl { get; set; }
        
        
        [MinLength(5)]
        [MaxLength(64)]
        public string? WebSocketId { get; set; }
        
        public bool EmailVerified { get; set; } = default!;

        [MaxLength(64)]
        public string? VerificationLink { get; set; } 
        
        
        public NotificationSettings NotificationSettings { get; set; } = default!;

    }
}