using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using DAL;
using DTO;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;

namespace Rocket_chat_api.Helper
{
    public static class TokenValidation
    {
        private static JwtSecurityTokenHandler jwtHandler { get; set; }
        static TokenValidation()
        {
            jwtHandler = new JwtSecurityTokenHandler();
        }

        internal static Dictionary<string,string> ValidateToken(string token,string signature)
        {
            var readableToken = jwtHandler.CanReadToken(token);

            if (readableToken != true)
            {
                 throw new ArgumentException("The token doesn't seem to be in a proper JWT format.");
            }
            try
            {
                var claimsPrincipal = jwtHandler.ValidateToken(token,GetValidationParameters(signature), out var validatedToken);
                return claimsPrincipal.Claims.ToDictionary(c => c.Type, c => c.Value);
            }
            catch (Exception e)
            {
                throw new ArgumentException("Token validation failed");
            }
        }
        
        internal static async Task<string> CreateJwtAsync(UserDTO userDto,string signature )
        {
            var claims = await CreateClaimsIdentitiesAsync(userDto);

            // Create JWToken
            var token = jwtHandler.CreateJwtSecurityToken(
                subject: claims,
                notBefore: new DateTime(1970, 1, 1),
                expires: DateTime.UtcNow.AddDays(1),
                signingCredentials:
                new SigningCredentials(
                    new SymmetricSecurityKey(
                        Encoding.Default.GetBytes(signature)),
                    SecurityAlgorithms.HmacSha256Signature));

            return jwtHandler.WriteToken(token);
        }
        
        private static Task<ClaimsIdentity> CreateClaimsIdentitiesAsync(UserDTO userDto)
        {
            var claimsIdentity = new ClaimsIdentity();
            claimsIdentity.AddClaim(new Claim("userName",userDto.UserName ));
            claimsIdentity.AddClaim(new Claim("userId", userDto.UserId.ToString()));
            claimsIdentity.AddClaim(new Claim("imageUrl",userDto.ImageUrl ));
            claimsIdentity.AddClaim(new Claim("isOnline",userDto.IsOnline.ToString() ));
            claimsIdentity.AddClaim(new Claim("notificationSettings", JsonConvert.SerializeObject(new
            {
                notificationSettingsId = userDto.NotificationSettings.NotificationSettingsId,
                sound = userDto.NotificationSettings.Sound,
                newMessageReceived = userDto.NotificationSettings.NewMessageReceived,
                newChatReceived = userDto.NotificationSettings.NewChatReceived,
                connectionChanged = userDto.NotificationSettings.ConnectionChanged,
            })));

            return Task.FromResult(claimsIdentity);
        }
        private static TokenValidationParameters GetValidationParameters(string signature)
        {
            // Validate tokens received from client, only signature matters here
            return new TokenValidationParameters()
            {
                ValidateLifetime = false, // Because there is no expiration in the generated token
                ValidateAudience = false, // Because there is no audiance in the generated token
                ValidateIssuer = false,   // Because there is no issuer in the generated token
                ValidIssuer = "Sample",
                ValidAudience = "Sample",
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signature)) // The same key as the one that generate the token
            };
        }
    }
}