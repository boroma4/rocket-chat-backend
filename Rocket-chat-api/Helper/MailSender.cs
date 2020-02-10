using System;
using System.Net;
using System.Net.Mail;
using MailAddress = System.Net.Mail.MailAddress;
using SmtpClient = System.Net.Mail.SmtpClient;

namespace Rocket_chat_api
{
    public static class MailSender
    {
        public static void SendEmail(string recipient, string link,string password)
        {
            try
            {
                var mailMessage = new MailMessage {From = new MailAddress("noreply.rocketchat@gmail.com")};

                //receiver email address
                mailMessage.To.Add(recipient);

                //subject of the email
                mailMessage.Subject = "Please confirm your email";

                mailMessage.Body = "Confirm your email by clicking this link " + link ;
                mailMessage.IsBodyHtml = true;

                //SMTP client
                var smtpClient = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential("noreply.rocketchat@gmail.com",password),
                    EnableSsl = true
                };

                //enabled SSL
                //Send an email
                smtpClient.Send(mailMessage);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}