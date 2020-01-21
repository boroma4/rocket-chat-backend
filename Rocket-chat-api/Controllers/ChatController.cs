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

        
        /// <summary>
        /// Function to process adding of the new chat with a user.
        /// </summary>
        /// <param name="curUserId"> Id of the user who initiates new chat creation</param>
        /// <param name="emailToAdd"> Email of the person who is to be added to the new chat</param>
        /// <returns>Chat ID and username to be displayed</returns>
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

        /// <summary>
        /// A function to get all chats when the user logs in together with their last message.
        /// Ideally should also return the name of the chat, but it seems our database is not good enough yet.
        /// </summary>
        /// <param name="userId">Id of the user that logs in</param>
        /// <returns>List of Chat+LastMessage elements in JSON notation as a response</returns>
        [HttpGet]
        [Route("/api/getallchats")]
        public IActionResult GetAllChatsWithLastMessage(int userId)
        {
            Console.Write(userId);
            List<ChatUser> chatsOfUser = new List<ChatUser>();
            chatsOfUser = _context.ChatUsers.Where(user => user.UserId == userId).ToList();
            List<UserChat> userChatsToReturn = new List<UserChat>();
            Console.Write(userChatsToReturn.Count);
            
            for (int i = 0; i < chatsOfUser.Count; i++)
            {
                userChatsToReturn.Add(new UserChat()
                {
                    ChatId = chatsOfUser[i].ChatId,
                    LastMessage = _context.Messages.FirstOrDefault(message => message.ChatId == chatsOfUser[i].ChatId)
                });
            }
            Console.Write("Success");
            return Ok(userChatsToReturn);
        }

        /// <summary>
        /// Method to get last 10 messages for a specific chat.
        /// Will add actually last messages as soon as we have timestamp on messages and can sort them accordingly
        /// </summary>
        /// <param name="chatId">Chat that we are trying to add to</param>
        /// <returns>List of for a chat</returns>
        [HttpGet]
        [Route("/api/getlastmessages")]
        public IActionResult GetLastTenMessages(int chatId)
        {
            List<Message> messages = new List<Message>();
            messages = _context.Messages.Where(message => message.ChatId == chatId).Take(10).ToList();
            return Ok(messages);
        }
    }
}