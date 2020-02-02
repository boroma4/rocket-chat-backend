using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DAL;
using Domain;
using DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Rocket_chat_api.Hubs;

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
        /// <returns>Chat ID and username to be displayed. If there re any logical errors, nothing is returned</returns>
        [HttpGet]
        [Route("/api/addchat")]
        public IActionResult AddChat(int curUserId,string emailToAdd)
        {
            var match =_context.Users.SingleOrDefault(u => u.Login.Email.Equals(emailToAdd));
            if (match == null)
            {
                return BadRequest(new {text = "No one with such email exists bro..."});
            }
            
            //Checking if user is not adding himself 
            if (match.UserId == curUserId)
            {
                return BadRequest(new {text = "You cannot add yourself,pls.."});
            }
            
            var curUser = _context.Users.Find(curUserId);
            
            
            var chatsCurrentUserHas = _context.ChatUsers.Where(user => user.UserId == curUserId).ToList();
            
            // Loop looking for occurrences, when we already have a same chat ID with the user that we are about to add
            foreach (var t in chatsCurrentUserHas)
            {
                var userListPerChat = ChatUserForASpecificChatFinder(t.ChatId);
                if (userListPerChat.Any(user => user.UserId == match.UserId))
                {
                    return BadRequest(new {text = "You already have this guy..."});
                }
            }

            if (match != null && curUser != null)
            {
                
                var chatUser =_chatWorker.AddChat(new List<User>(){curUser,match});
                
                var newChatDto = new AddChatDto
                {
                    ChatId = chatUser.ChatId, UserName = chatUser.User.UserName,FriendImageUrl = chatUser.User.ImageUrl
                };

                return Ok(newChatDto);
            }
            return BadRequest(new{text = "Email not found"});
        }

        /// <summary>
        /// A function to get all chats when the user logs in together with their last message.
        /// Ideally should also return the name of the chat, but it seems our database is not good enough yet.
        /// </summary>
        /// <param name="userId">Id of the user that logs in</param>
        /// <returns>List of Chat+LastMessage elements in JSON notation as a response</returns>
        [HttpGet]
        [Route("/api/getallchats")]
        public async Task<IActionResult> GetAllChatsWithLastMessage(int userId)
        {
            var chatsOfUser = _context.ChatUsers.Where(user => user.UserId == userId).ToList();
            var userChatsToReturn = new List<UserChatDTO>();
            Console.Write(userChatsToReturn.Count);
            
            
            for (int i = 0; i < chatsOfUser.Count; i++)
            {
                var friendId = _context.ChatUsers
                    .Single(user => user.ChatId == chatsOfUser[i].ChatId && user.UserId != userId).UserId;
                
                var friend = _context.Users.Find(friendId);
                
                //Trying to use LastOrDefault resulted in a crash, using this workaround for now 
                var msgList = await _context.Messages.Where(message => message.ChatId == chatsOfUser[i].ChatId).ToListAsync();
                Message? lastMsg = msgList.Count > 0 ? msgList[^1] : null;
                
                userChatsToReturn.Add(new UserChatDTO()
                {
                    ChatId = chatsOfUser[i].ChatId,
                    LastMessage = lastMsg,
                    FriendUserName = friend.UserName,
                    IsOnline = friend.IsOnline,
                    FriendImageUrl = friend.ImageUrl
                });
            }

            var emptyChats = new List<UserChatDTO>();
            
            if (userChatsToReturn.Any(dto => dto.LastMessage == null))
            { 
                emptyChats = userChatsToReturn.Where(dto => dto.LastMessage == null).ToList();
                userChatsToReturn.RemoveAll(dto => dto.LastMessage == null);
            }

            userChatsToReturn = userChatsToReturn.OrderByDescending(dto => dto.LastMessage.CreatedDate).ToList();
            
            if (emptyChats.Count != 0)
            {
                userChatsToReturn.AddRange(emptyChats);
            }

            return Ok(userChatsToReturn);
        }

        /// <summary>
        /// Method to get last 10 messages for a specific chat.
        /// Will skip those that are already present in chat.
        /// </summary>
        /// <param name="chatId">Chat that we are trying to add to</param>
        /// <param name="totalMessagesLoaded">id for checking the last message</param>
        /// <returns>List of for a chat</returns>
        [HttpGet]
        [Route("/api/getlastmessages")]
        public IActionResult GetLastTenMessages(int chatId, int totalMessagesLoaded)
        {
                var messages = _context.Messages.Where(message => message.ChatId == chatId)
                    .OrderByDescending(message => message.CreatedDate)
                    .Skip(totalMessagesLoaded)
                    .Take(10)
                    .OrderBy(message => message.CreatedDate)
                    .ToList();
                return Ok(messages);
            
        }
        

        /// <summary>
        /// A helper function that returns ChatUser dependency associated with a specific chat
        /// </summary>
        /// <param name="chatId">A chat in question</param>
        /// <returns> List of Users</returns>
        ///
        /// Move to ChatWorker pls
        [NonAction]
        private List<ChatUser> ChatUserForASpecificChatFinder(int chatId)
        {
            var chatUserList = _context.ChatUsers.Where(user => user.ChatId == chatId).ToList();
            return chatUserList;
        }
    }
}