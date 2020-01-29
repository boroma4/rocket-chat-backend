using System;
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
    
    /// <summary>
    /// Class used for controlling the Login/Register page
    /// </summary>
    [ApiController]
    public class LoginController : ControllerBase
    {

        private readonly ILogger<LoginController> _logger;
        
        private readonly AppDbContext _context;


        public LoginController(ILogger<LoginController> logger,AppDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        [HttpPost]
        [Route("/api/login")]
        public IActionResult Login(Login loginData)
        {
            if (!ModelState.IsValid || string.IsNullOrEmpty(loginData.Email) || string.IsNullOrEmpty(loginData.Password))
             return BadRequest(new {text ="Invalid data"});

            var match = _context.Logins.SingleOrDefault(l=>l.Email.Equals(loginData.Email));

            if (match == null) return BadRequest(new {text = "Wrong email or password"});
            
            if (!PasswordSecurity.CheckPassword(match.Password, loginData.Password))
                return BadRequest(new {text = "Wrong email or password"});
                
            var user = _context.Users.SingleOrDefault(u => u.Login.Email.Equals(match.Email));
            //IsOnline will be set on socket connection
            if (user != null)
            {
                return Ok(new UserDTO
                {
                    UserId = user.UserId,
                    UserName = user.UserName,
                    IsOnline = false
                });
            }
            return BadRequest(new {text = "Wrong email or password"});
        }
        
        [HttpPost]
        [Route("/api/register")]
        public async Task<IActionResult> RegisterUser(Login loginData)
        {
            if (!ModelState.IsValid || string.IsNullOrEmpty(loginData.Email) || string.IsNullOrEmpty(loginData.Password)|| string.IsNullOrEmpty(loginData.UserName))
                return BadRequest(new{text = "Invalid data."});

            if (_context.Users.Any(u => u.Login.Email.Equals(loginData.Email)))
            {
                return BadRequest( new {text = "Email already exists"});
            }
            
            loginData.Password = PasswordSecurity.Encrypt(loginData.Password);
            
            var newUser = new User()
            {
                Login = loginData,
                UserName = loginData.UserName
            };
            
            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();
            //IsOnline will be set on socket connection

            return Ok(new UserDTO
            {
                UserId = newUser.UserId,
                UserName = newUser.UserName,
                IsOnline = false
            });
        }
    }
}