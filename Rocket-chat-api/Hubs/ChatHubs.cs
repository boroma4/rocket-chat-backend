using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace Rocket_chat_api.Hubs
{
    
    /// <summary>
    /// Class to control message sending
    /// </summary>
    public class ChatHub : Hub
        {
            public async Task SendMessage(int userId, string message)
            {
                await Clients.All.SendAsync("sendToAll", userId, message);
            }
        }
}