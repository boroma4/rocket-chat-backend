using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using DAL;
using Domain;

namespace Rocket_chat_api.Hubs
{
    public class ChatHub : Hub
        {
            private readonly AppDbContext _context;
            
            public ChatHub(AppDbContext context)
            {
                _context = context;
            }
            //TODO send message to specific client
            public async Task SendDirectMessage(int userId, int chatId, string messageText)
            {
                
                await Clients.All.SendAsync("sendToAll", userId, chatId, messageText);
                Message newMessage = new Message(userId,chatId,messageText);
                if (!_context.Chats.Any(chat => chat.ChatId == chatId))
                {
                    _context.Chats.Add(new Chat(){ChatId = chatId});
                }
                _context.Messages.Add(newMessage);
                await _context.SaveChangesAsync();

            }
            
            
            
            //TODO notify clients that have chats with current user that he is online (connected)
        }
}