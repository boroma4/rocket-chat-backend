using System;
using System.Collections.Generic;
using System.Linq;
using DAL;
using Domain;
using DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Rocket_chat_api.Controllers
{
    [ApiController]

    public class StatusController : ControllerBase
    {
        private readonly ILogger<LoginController> _logger;
        
        private readonly AppDbContext _context;


        public StatusController(ILogger<LoginController> logger,AppDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        /// <summary>
        /// Method that sets a user as offline in the database.
        /// </summary>
        /// <param name="userId">Id of the user that went offline</param>
        [HttpGet]
        [Route("/api/disconnect")]
        public IActionResult UserDisconnected(int userId)
        {
            var user = _context.Users.Find(userId);
            if (user == null)
            {
                return NotFound();
            }
            
            user.IsOnline = false; 
            _context.Update(user); 
            _context.SaveChanges(); 
            return Ok();
         }


        /// <summary>
        /// Method for chats 1 to 1 for finding out the status of user (Is he online or offline)
        /// </summary>
        /// <param name="userId"> Id of the user that Gets the request</param>
        /// <param name="chatId"> Array of chats user currently has</param>
        /// <returns> UserStatusDTO with ChatId and Status of user</returns>
        [HttpGet]
        [Route("api/checkactivity")]
        public IActionResult CheckUsersActivity(int userId, ICollection<int> chatId)
        {
            var statuses = new List<UserStatusDTO>();

            try
            {
                foreach (var entry in chatId)
                {
                    statuses.Add(new UserStatusDTO()
                    {
                        ChatId = entry,
                        UserActivity = _context.ChatUsers.Single(user => user.ChatId == entry && user.UserId !=userId).User.IsOnline
                    });
                }
            
                return Ok(statuses);
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }
        }
        /// <summary>
        /// Method to verify user's email
        /// </summary>
        /// <param name="key"> Secret code generated based of user email</param>
        /// <returns> Redirects to correct page in front-end</returns>
        [HttpGet]
        [Route("api/verify")]
        public IActionResult VerifyEmail(string key)
        {
            var verifiedUser = _context.Users.SingleOrDefault(u => u.VerificationLink ==  key);

            if (verifiedUser != null)
            {
                verifiedUser.EmailVerified = true;
                _context.Users.Update(verifiedUser);
                _context.SaveChanges();
                return Redirect("http://localhost:3000/rocket-chat-front/#/vsuccess");
            }
            return Redirect("http://localhost:3000/rocket-chat-front/#/vfailed");
        }


        [HttpGet]
        [Route("api/settingsapply")]
        public IActionResult SaveNotificationSettings(int userId, bool sound, bool newChat, bool newMessage, bool connection)
        {
            var notificationId = _context.Users.Single(user => user.UserId == userId).NotificationSettingsId;
            var notificationEntity = _context.NotificationSettings.SingleOrDefault(settings => settings.NotificationSettingsId == notificationId);

            if (notificationEntity!=null)
            {
                notificationEntity.Sound = sound;
                notificationEntity.ConnectionChanged = connection;
                notificationEntity.NewChatReceived = newChat;
                notificationEntity.NewMessageReceived = newMessage;
                _context.NotificationSettings.Update(notificationEntity);
                _context.SaveChanges();
            }
            
            return Ok("success");
        }
    }
}