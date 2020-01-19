using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DAL;
using Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Rocket_chat_api.Controllers
{
    [ApiController]
    public class ChatController : ControllerBase
    {

        private readonly ILogger<LoginController> _logger;
        
        private readonly AppDbContext _context;
        
        private ChatWorker _chatWorker ;



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
            var chatId = -1;
            var match =_context.Users.Where(u => u.Login.Email.Equals(emailToAdd)).ToList();
            if (match.Count > 0)
            {
                var curUser = _context.Users.Find(curUserId);
                chatId =_chatWorker.AddChat(new List<User>(){curUser,match[0]});
                
                return Ok(chatId);
            }
            return BadRequest("Email not found");
        }
        //TODO get messages by chat Id
    }
}