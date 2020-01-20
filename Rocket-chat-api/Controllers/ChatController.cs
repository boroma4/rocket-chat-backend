using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DAL;
using Domain;
using DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Rocket_chat_api.Controllers
{
    [ApiController]
    public class ChatController : ControllerBase
    {

        private readonly ILogger<LoginController> _logger;
        
        private readonly AppDbContext _context;
        
        private ChatWorker _chatWorker;
        
        public ChatController(ILogger<LoginController> logger,AppDbContext context)
        {
            _logger = logger;
            _context = context;
            _chatWorker = new ChatWorker(_context);
        }

        [HttpGet]
        [Route("/api/addchat")]
        public IActionResult AddChat(int curUserId,string emailToAdd)
        {
            ChatUser chatUser;
            var match =_context.Users.Single(u => u.Login.Email.Equals(emailToAdd));
            
            if (match != null)
            {
                var curUser = _context.Users.Find(curUserId);
                
                chatUser =_chatWorker.AddChat(new List<User>(){curUser,match});
                
                return Ok(new AddChatDto
                {
                    ChatId = chatUser.ChatId,
                    UserName = chatUser.User.UserName
                });
            }
            return BadRequest("Email not found");
        }
        //TODO get messages(all or at least last 10) by chat Id
        //TODO get all chatIds + last message by userId
    }
}