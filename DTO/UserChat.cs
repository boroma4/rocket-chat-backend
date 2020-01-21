using DAL;

namespace DTO
{
    public class UserChat
    {
        public int? ChatId { get; set; }
        public Message? LastMessage { get; set; }
    }
}