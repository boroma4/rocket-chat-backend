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

        [HttpPost]
        [Route("/api/google")]
        public async Task<IActionResult> HandleGoogleLogin(GoogleTokenDTO tokenDto)
        {

            if (!ModelState.IsValid || string.IsNullOrEmpty(tokenDto.GoogleToken))
                return BadRequest(new {text = "Invalid data."});
            
            var googleToken = tokenDto.GoogleToken;
            
            var jwtHandler = new JwtSecurityTokenHandler();

            var readableToken = jwtHandler.CanReadToken(googleToken);

            if (readableToken != true)
            {
                return BadRequest(new {text = "The token doesn't seem to be in a proper JWT format."});
            }

            var token = jwtHandler.ReadJwtToken(googleToken);

            //Extract the payload of the JWT
            var claims = token.Claims;
            var claimDictionary = new Dictionary<string, string>();

            //turn IEnumerable to collection
            foreach (var c in claims)
            {
                claimDictionary.Add(c.Type, c.Value);
                Console.WriteLine(c.Type);
            }

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
                return Ok(new UserDTO
                {
                    UserId = user.UserId,
                    UserName = user.UserName,
                    IsOnline = false
                });
            }
            else
            {
                var loginData = new Login() {Email = email,Password = "xxxIsGoogleGringoXxx"};
                var user = new User()
                {
                    Login = loginData,
                    UserName = name,
                    ImageUrl = claimDictionary["picture"]
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                
                return Ok(new UserDTO
                {
                    UserId = user.UserId,
                    UserName = user.UserName,
                    IsOnline = false
                });

            }

        }
    }
}