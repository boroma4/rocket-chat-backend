using System;
using System.Security.Cryptography;
using System.Text;

namespace Rocket_chat_api
{
    
    /// <summary>
    /// Class used to hash the password in order for it to be stored in the database
    /// </summary>
    public  static class Security
    {
        internal static string Encrypt(string password,int iterations)
        {
            byte[] salt;
            new RNGCryptoServiceProvider().GetBytes(salt=new byte[16]);
            
            var pbkdf2 = new Rfc2898DeriveBytes(password,salt,iterations);

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
        internal static string ComputeSha256Hash(string rawData)
        {
            // Create a SHA256   
            using var sha256Hash = SHA256.Create();
            // ComputeHash - returns byte array  
            var bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));  
  
            // Convert byte array to a string   
            var builder = new StringBuilder();  
            foreach (var b in bytes)
            {
                builder.Append(b.ToString("x2"));
            }  
            return builder.ToString();
        }  

    }
}