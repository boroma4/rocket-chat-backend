using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DAL;
using Domain;

namespace Rocket_chat_api
{
    public class ChatWorker
    {
        public ChatWorker(AppDbContext context)
        {
            _context = context;
        }
 
        private readonly AppDbContext _context;
        
        public ChatUser AddChat(ICollection<User> users)
        {

            //TODO verify that chat with same members does not exist yet
            
            var chatToAdd = new Chat();
            _context.Chats.Add(chatToAdd);

            var addedChatUsers= new List<ChatUser>(2);
            
            foreach (var user in users)
            {
                var ch = (new ChatUser
                {
                    User = user,
                    Chat = chatToAdd
                });
                _context.ChatUsers.Add(ch);
                
                //hacky way to get ChatUser with second user
                addedChatUsers.Add(ch);
            }
            _context.SaveChanges();

            //first user is the one who sent request, second is the one to add
            return addedChatUsers[1];
        }
        
        /*
        public bool ChatExists(int senderId, int receiverId)
        {
            var senderChatUser = _context.ChatUsers.FirstOrDefault(c=>c.User.UserId == senderId);
            if(senderChatUser != null)
            {
                var workingChat = senderChatUser.Chat;
                var receiverChatUser = _context.ChatUsers.Where(c=>c.UserId == receiverId).ToList();
                if (receiverChatUser[0] != null)
                {
                    return workingChat == receiverChatUser[0].Chat;
                }
            }
            return false;
        }
        */
    }
}