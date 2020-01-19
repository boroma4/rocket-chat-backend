using System.Collections;
using System.Collections.Generic;
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
        
        public void AddChat(ICollection<User> users)
        {
            var chatToAdd = new Chat();
            _context.Chats.Add(chatToAdd);
            _context.SaveChanges();

            foreach (var user in users)
            {
                _context.Add(new ChatUser
                {
                    User = user,
                    Chat = chatToAdd
                });
            }
            _context.SaveChanges();
        }
    }
}