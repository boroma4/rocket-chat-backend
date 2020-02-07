
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using DAL;
using Domain;
using Microsoft.EntityFrameworkCore;

namespace Rocket_chat_api.Hubs
{
    public class ChatHub : Hub
        {
            private readonly AppDbContext _context;
            
            public ChatHub(AppDbContext context)
            {
                _context = context;
            }
            public async Task SendDirectMessage(int userId, int chatId, string messageText)
            {
                var newMessage = new Message(userId,chatId,messageText);

                _context.Messages.Add(newMessage);

                var chatUserToFind = await _context.ChatUsers.FirstOrDefaultAsync(c => c.ChatId == chatId && c.UserId != userId);
                var friend = await _context.Users.FindAsync(chatUserToFind.UserId);
                    
                await Clients.Client(friend.WebSocketId).SendAsync("sendDirectMessage", userId, chatId, messageText);
                await _context.SaveChangesAsync();
                    
            }

            /// <summary>
            /// A function to create a new Chat with a user that was added as a friend.
            /// Works as event handler for Receiving user.
            /// Might be reworked to be fully operating the process
            /// of new chat creation.
            /// </summary>
            /// <param name="chatId">id of created chat</param>
            /// <param name="currentUserId">id of the user, who is creating a new chat</param>
            /// <param name="newChat">the chat object itself</param>
            /// <returns></returns>
            /// current user id was there for some reason 
            public async Task ChatWithUserWasCreated(int currentUserId,int chatId, object newChat)
            {
                var chatUserToAdd = _context.ChatUsers.SingleOrDefault(ch => ch.UserId != currentUserId && ch.ChatId == chatId);
                if (chatUserToAdd != null)
                {
                    var userToAddId = chatUserToAdd.UserId;
                    var userToAddSocket = _context.Users.Find(userToAddId).WebSocketId;
                    if (userToAddSocket != null)
                    {
                        await Clients.Client(userToAddSocket).SendAsync("getChat",newChat);
                    }
                }
            }

            /// <summary>
            /// A function that notifies all the clients that a certain user went online or offline.
            /// Chat Ids are sent to check whether receiver was present in client's chats.
            /// </summary>
            /// <param name="online">bool that shows whether user should be put online or offline</param>
            /// <param name="userId">id of the user, who is going online or offline</param>
            /// <returns></returns>
            public async Task UserWentOfflineOrOnline(bool online,int userId,string? webSocketId)
            {
                var currentUser = _context.Users.Find(userId);
                var userChats = _context.ChatUsers.Where(ch => ch.UserId == userId);
                var chatIdList = userChats.Select(userChat => userChat.ChatId).ToList();
                
                currentUser.IsOnline = online;
   
                currentUser.WebSocketId = online ? webSocketId : null;
                
                await Clients.All.SendAsync("UserWentOfflineOrOnline",online,userId,chatIdList);
                _context.Users.Update(currentUser);
                await _context.SaveChangesAsync();
            }

            /// <summary>
            /// A function that notifies all the clients that a certain user changed something about him.
            /// </summary>
            ///<param name="userId">Id of a user who made changes</param>
            /// <param name="type">string that shows what exactly the user has changed</param>
            /// <param name="value">string that shows what value did the user put</param>
            /// <returns></returns>
            public async Task UserDataChanged(int userId ,string type, string value)
            {
                var user = _context.Users.Find(userId);
                //if data is broken/incorrect abort changes
                if (user == null || string.IsNullOrEmpty(type) || string.IsNullOrEmpty(value))
                {
                    return;
                }
                switch (type)
                {
                    case "image":
                        user.ImageUrl = value;
                        break;
                    case "name":
                        user.UserName = value;
                        break;
                }
                _context.Users.Update(user);
                
                var userChatIds = await _context.ChatUsers.Where(cu => cu.UserId == userId)
                    .Select(cu => cu.ChatId)
                    .ToListAsync();
                
                await Clients.All.SendAsync("UserDataChanged",userId,userChatIds,type,value);
                await _context.SaveChangesAsync();
            }
        }
}