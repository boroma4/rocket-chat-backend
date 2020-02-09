using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Rocket_chat_api
{
    public class DataEncryption
    {
        public static string EncryptionToString(string messageText)
        {
            using(AesManaged aes = new AesManaged()) {  

                Console.WriteLine(Convert.ToBase64String(aes.Key));
                Console.WriteLine(Convert.ToBase64String(aes.IV));
                
                var byteConverter = new UnicodeEncoding();
                var message = Encoding.UTF8.GetBytes(messageText);
                messageText = Encoding.UTF8.GetString(message);
                //For production use Key and IV have to be stored in Environmental variables
                //THESE KEY AND IV ARE FOR TESTING USAGE ONLY
                var encryptor = aes.CreateEncryptor(
                    Convert.FromBase64String("3ICSVK1JfR+GBzw/iilv+/gttcRwxUYZI0XxJkqWdJA="),
                    Convert.FromBase64String("4hayN7sv3Jma/85LhnKSJQ=="));  

                using(MemoryStream ms = new MemoryStream()) {  

                    using(CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write)) {

                        using (StreamWriter writer = new StreamWriter(cs))
                        {
                            writer.Write(messageText);  

                        }  
                    }  
                }
            }
            
            return messageText;
        }
        
        
        
        /// <summary>
        /// Class for Decrypting messages that are sent from the client
        /// </summary>
        /// <param name="messageText">text of message that is being sent</param>
        /// <returns></returns>
        public static string DecryptionFromString(string messageText)
        {
            using(AesManaged aes = new AesManaged()) {  

                var byteConverter = new UnicodeEncoding();
                var message = byteConverter.GetBytes(messageText);
                    
                //For production use Key and IV have to be stored in Environmental variables
                //THESE KEY AND IV ARE FOR TESTING USAGE ONLY
                var decryptor = aes.CreateDecryptor(
                    Convert.FromBase64String("3ICSVK1JfR+GBzw/iilv+/gttcRwxUYZI0XxJkqWdJA="),
                    Convert.FromBase64String("4hayN7sv3Jma/85LhnKSJQ=="));  

                using(MemoryStream ms = new MemoryStream(Convert.FromBase64String(messageText))) {  

                    using(CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read)) {  

                        using(StreamReader reader = new StreamReader(cs))  
                            messageText = reader.ReadToEnd();  
                    }  
                } 
                    
                    
            }

            Console.WriteLine(messageText);
            return messageText;
        }

        /// <summary>
        /// Generic method for decrypting JSON objects sent
        /// </summary>
        /// <param name="jsonObjectEncrypted">JSON object to be decrypted</param>
        /// <typeparam name="T">Any object that can be decrypted</typeparam>
        /// <returns>decrypted JSON</returns>
        public static T DecryptionFromJSON<T>(string jsonObjectEncrypted)
        {
            using(AesManaged aes = new AesManaged()) {  

                var byteConverter = new UnicodeEncoding();
                var message = byteConverter.GetBytes(jsonObjectEncrypted);
                    
                //For production use Key and IV have to be stored in Environmental variables
                //THESE KEY AND IV ARE FOR TESTING USAGE ONLY
                var decryptor = aes.CreateDecryptor(
                    Convert.FromBase64String("3ICSVK1JfR+GBzw/iilv+/gttcRwxUYZI0XxJkqWdJA="),
                    Convert.FromBase64String("4hayN7sv3Jma/85LhnKSJQ=="));  

                using(MemoryStream ms = new MemoryStream(Convert.FromBase64String(jsonObjectEncrypted))) {  

                    using(CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read)) {  

                        using(StreamReader reader = new StreamReader(cs))  
                            jsonObjectEncrypted = reader.ReadToEnd();  
                    }  
                }
            }

            var objectToReturn = JsonSerializer.Deserialize<T>(jsonObjectEncrypted);
            
            return objectToReturn;
        }

    }
}