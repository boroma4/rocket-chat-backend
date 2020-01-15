using System;
using System.Security.Cryptography;

namespace Rocket_chat_api
{
    public  static class PasswordSecurity
    {
        internal static string Encrypt(string password)
        {
            byte[] salt;
            new RNGCryptoServiceProvider().GetBytes(salt=new byte[16]);
            
            var pbkdf2 = new Rfc2898DeriveBytes(password,salt,1000);

            var hash = pbkdf2.GetBytes(20);
            var hashBytes = new byte[36];
            
            Array.Copy(salt,0,hashBytes,0,16);
            Array.Copy(hash,0,hashBytes,16,20);

            return Convert.ToBase64String(hashBytes);

        }

        internal static bool CheckPassword(string hashedPassword,string enteredPassword)
        {
            var hashBytes = Convert.FromBase64String(hashedPassword);
            var salt = new byte[16];
            Array.Copy(hashBytes,0,salt,0,16);
            
            var pbkdf2 = new Rfc2898DeriveBytes(enteredPassword,salt,1000);
            
            var hash = pbkdf2.GetBytes(20);

            for (var i = 0; i < 20; i++)
            {
                if (hashBytes[i + 16] != hash[i])
                {
                    return false;
                }
            }

            return true;
        }

    }
}