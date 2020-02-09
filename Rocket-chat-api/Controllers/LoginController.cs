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


        public LoginController(ILogger<LoginController> logger,AppDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        [HttpPost]
        [Route("/api/login")]
        public async Task<IActionResult> Login(Login loginData)
        {
            if (!ModelState.IsValid || string.IsNullOrEmpty(loginData.Email) || string.IsNullOrEmpty(loginData.Password))
             return BadRequest(new {text ="Invalid data"});

            var match = _context.Logins.SingleOrDefault(l=>l.Email.Equals(loginData.Email));

            if (match == null) return BadRequest(new {text = "Wrong email or password"});
            
            if (!Security.CheckPassword(match.Password, loginData.Password))
                return BadRequest(new {text = "Wrong email or password"});
                
            var user = _context.Users.SingleOrDefault(u => u.Login.Email.Equals(match.Email));
            
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
                var userToken = await TokenValidation.CreateJwtAsync(userData);
                return Ok(new {userToken});
            }
            return BadRequest(new {text = "Wrong email or password"});
        }
        
        [HttpPost]
        [Route("/api/register")]
        public async Task<IActionResult> RegisterUser(Login loginData)
        {
            _context.Database.EnsureCreated();
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
                MailSender.SendEmail(loginData.Email, secretLink);
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
            
            var googleToken = tokenDto.Token;
            
            var jwtHandler = new JwtSecurityTokenHandler();

            var readableToken = jwtHandler.CanReadToken(googleToken);

            if (readableToken != true)
            {
                return BadRequest(new {text = "The token doesn't seem to be in a proper JWT format."});
            }

            var token = jwtHandler.ReadJwtToken(googleToken);

            //Extract the payload of the JWT
            var claims = token.Claims;
            var claimDictionary = claims.ToDictionary(c => c.Type, c => c.Value);

            //turn IEnumerable to collection

            if (claimDictionary["iss"] != "accounts.google.com")
            {
                return BadRequest(new {text = "Why are you trying to hack us :("});
            }

            if (claimDictionary["email_verified"] != "true")
            {
                return BadRequest(new {text = "Please verify your Google email first."});
            }

            var email = claimDictionary["email"];
            var name = claimDictionary["given_name"];
            //user exists -> login
            if (_context.Users.Any(u => u.Login.Email.Equals(email)))
            {
                var user = _context.Users.Single(u => u.Login.Email.Equals(email));
                //we dont put our users as verified in the DB
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
                var userToken = await TokenValidation.CreateJwtAsync(userData);
                return Ok(new {userToken});
            }
            else
            {
                var loginData = new Login() {Email = email,Password = "xxxIsGoogleGringoXxx"};
                var notificationSetting = new NotificationSettings();
                _context.NotificationSettings.Add(notificationSetting);
                await _context.SaveChangesAsync();

                var user = new User()
                {
                    Login = loginData,
                    UserName = name,
                    ImageUrl = claimDictionary["picture"],
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
                var userToken = await TokenValidation.CreateJwtAsync(userData);
                return Ok(new {userToken});
            }

        }
    }
}