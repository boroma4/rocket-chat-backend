using System;
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
                Console.Write("First");
                await Clients.All.SendAsync("sendToAll", userId, chatId, messageText);
                Message newMessage = new Message(userId,chatId,messageText);
                Console.Write(messageText);
                if (!_context.Chats.Any(chat => chat.ChatId == chatId))
                {
                    _context.Chats.Add(new Chat(){ChatId = chatId});
                }
                _context.Messages.Add(newMessage);
                await _context.SaveChangesAsync();

            }

            /// <summary>
            /// A function to create a new Chat with a user that was added as a friend.
            /// Works as event handler for Receiving user.
            /// Might be reworked to be fully operating the process
            /// of new chat creation.
            /// </summary>
            /// <param name="emailToAdd">an email that is entered when we add the user as a friend</param>
            /// <param name="currentUserId">id of the user, who is creating a new chat</param>
            /// <param name="newChat">the chat object itself</param>
            /// <returns></returns>
            public async Task NewWebSocketChatCreated(string emailToAdd, int currentUserId, object newChat)
            {
                var userToAddId = -1;
                var userToAdd = _context.Logins.SingleOrDefault(login => login.Email.Equals(emailToAdd));
                if (userToAdd != null)
                {
                    userToAddId = userToAdd.LoginId;
                    userToAddId = _context.Users.Single(user => user.Login.LoginId == userToAddId).UserId;
                }
                await Clients.All.SendAsync("getChat",newChat);
            }

            
            
            //TODO notify clients that have chats with current user that he is online (connected)
        }
}