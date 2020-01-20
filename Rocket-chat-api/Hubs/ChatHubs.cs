using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace Rocket_chat_api.Hubs
{
    public class ChatHub : Hub
        {
            //TODO send message to specific client
            public async Task SendDirectMessage(string user, string message)
            {
                await Clients.All.SendAsync("sendToAll", user, message);
            }
            
            //TODO notify clients that have chats with current user that he is online (connected)
        }
}