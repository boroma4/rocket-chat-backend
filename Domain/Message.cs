namespace DAL
{
    public class Message
    {
        public int MessageId { get; set; }

        public Chat Chat { get; set; } = default!;
        //sender
        public int UserId { get; set; } = default!;

        public string MessageText { get; set; } = default!;
    }
}