using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace Rocket_chat_api.Hubs
{
    public class ChatHub : Hub
        {
            public async Task SendDirectMessage(string user, string message)
            {
                await Clients.All.SendAsync("sendToAll", user, message);
            }
        }
}