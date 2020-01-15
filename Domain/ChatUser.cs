namespace DAL
{
    public class ChatUser
    {
        public int ChatUserId { get; set; }

        public int ChatId { get; set; } = default!;

        public Chat? Chat { get; set; }

        public int UserId { get; set; } = default!;

        public User? User { get; set; }

    }
}