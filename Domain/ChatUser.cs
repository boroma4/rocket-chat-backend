using System.ComponentModel.DataAnnotations;

namespace DAL
{
    public class ChatUser
    {
        public int ChatUserId { get; set; }

        [Required]
        public int ChatId { get; set; } = default!;

        public Chat? Chat { get; set; }

        [Required]
        public int UserId { get; set; } = default!;

        public User? User { get; set; }

    }
}