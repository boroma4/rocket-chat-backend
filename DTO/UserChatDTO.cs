using DAL;

namespace DTO
{
    public class UserChatDTO
    {
        public int? ChatId { get; set; }
        public Message? LastMessage { get; set; }

        public string? FriendUserName { get; set; }
        
        public string? FriendImageUrl { get; set; }


        public bool? IsOnline { get; set; }
    }
    
}