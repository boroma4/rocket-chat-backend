using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using DAL;
using Domain;
using DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Permissions;
using Microsoft.Extensions.Configuration;
using Rocket_chat_api.Helper;

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

        private readonly IConfiguration _configuration;


        public LoginController(ILogger<LoginController> logger,AppDbContext context,IConfiguration configuration)
        {
            _configuration = configuration;
            _logger = logger;
            _context = context;
        }

        [HttpPost]
        [Route("/api/login")]
        public async Task<IActionResult> Login(Login loginData)
        {
            if (!ModelState.IsValid || string.IsNullOrEmpty(loginData.Email) || string.IsNullOrEmpty(loginData.Password))
             return BadRequest(new {text ="Invalid data"});

            var match = _context.Logins.SingleOrDefault(l=>l.Email == (loginData.Email));

            if (match == null) return BadRequest(new {text = "Wrong email or password"});
            
            if (match.Password == _configuration["GOOGLE_PASS"]) return BadRequest(new {text = "Please use Google to login"});

            
            if (!Security.CheckPassword(match.Password, loginData.Password))
                return BadRequest(new {text = "Wrong email or password"});
                
            var user = _context.Users.SingleOrDefault(u => u.Login.Email == (match.Email));
            
            //IsOnline will be set on socket connection
            if (user != null)
            {
                if (!user.EmailVerified)
                {
                    return BadRequest(new {text = "Please verify your email!"});
                }

                var notificationSetting = _context.NotificationSettings.Find(user.NotificationSettingsId);
                var userData = new UserDTO
                {
                    UserId = user.UserId,
                    UserName = user.UserName,
                    IsOnline = user.IsOnline,
                    ImageUrl = user.ImageUrl,
                    NotificationSettings = notificationSetting
                };
                var userToken = await TokenValidation.CreateJwtAsync(userData,_configuration["TOKEN_SIGNATURE"]);
                return Ok(new {userToken});
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
            
            loginData.Password = Security.Encrypt(loginData.Password,1000);
            var secretKey = Security.ComputeSha256Hash(loginData.Email);

            var notificationSetting = new NotificationSettings();
            _context.NotificationSettings.Add(notificationSetting);
            await _context.SaveChangesAsync();
            var newUser = new User()
            {
                Login = loginData,
                UserName = loginData.UserName,
                EmailVerified = false,
                VerificationLink = secretKey,
                NotificationSettingsId = notificationSetting.NotificationSettingsId,

            };
            _context.Users.Add(newUser);

            var secretLink = "https://rocket-chat-api.azurewebsites.net/api/verify?key=" + secretKey;
            try
            {
                MailSender.SendEmail(loginData.Email, secretLink,_configuration["NOREPLYPASS"]);
            }
            catch (Exception)
            {
                return BadRequest(new {text = "Registration failed, try again later!"});
            }
            await _context.SaveChangesAsync();

            return Ok(new {text = "User registered successfully"});
        }

        [HttpPost]
        [Route("/api/google")]
        public async Task<IActionResult> HandleGoogleLogin(TokenDto tokenDto)
        {
            if (!ModelState.IsValid || string.IsNullOrEmpty(tokenDto.Token))
                return BadRequest(new {text = "Invalid data."});
            
            var token = tokenDto.Token;
            Dictionary<string, string> validatedTokenClaims;
            try
            {
                validatedTokenClaims = TokenValidation.ValidateToken(token,_configuration["TOKEN_SIGNATURE"]);
            }
            catch (ArgumentException)
            {
                return BadRequest(new {text = "Validation failed"});
            }

            var email = validatedTokenClaims["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress"];
            var name = validatedTokenClaims["name"];
            var picture = validatedTokenClaims["imageUrl"];
            //user exists -> login
            if (_context.Users.Any(u => u.Login.Email.Equals(email)))
            {
                var user = _context.Users.Single(u => u.Login.Email.Equals(email));
                //we dont put our google users as verified in the DB
                //This can still get broken if normal email is registered, but not verified yet
                if (user.EmailVerified)
                {
                    return BadRequest(new {text = "Email already taken."});
                }
                var notificationSetting = _context.NotificationSettings.Find(user.NotificationSettingsId);
                var userData = new UserDTO
                {
                    UserId = user.UserId,
                    UserName = user.UserName,
                    IsOnline = user.IsOnline,
                    ImageUrl = user.ImageUrl,
                    NotificationSettings = notificationSetting
                };
                var userToken = await TokenValidation.CreateJwtAsync(userData,_configuration["TOKEN_SIGNATURE"]);
                return Ok(new {userToken});
            }
            else
            {
                var loginData = new Login() {Email = email,Password = _configuration["GOOGLE_PASS"]};
                var notificationSetting = new NotificationSettings();
                _context.NotificationSettings.Add(notificationSetting);
                await _context.SaveChangesAsync();

                var user = new User()
                {
                    Login = loginData,
                    UserName = name,
                    ImageUrl = picture,
                    NotificationSettingsId = notificationSetting.NotificationSettingsId
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                
                var userData = new UserDTO
                {
                    UserId = user.UserId,
                    UserName = user.UserName,
                    IsOnline = user.IsOnline,
                    ImageUrl = user.ImageUrl,
                    NotificationSettings = notificationSetting
                };
                var userToken = await TokenValidation.CreateJwtAsync(userData,_configuration["TOKEN_SIGNATURE"]);
                return Ok(new {userToken});
            }

        }
    }
}