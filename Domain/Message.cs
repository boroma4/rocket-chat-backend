using System;

namespace DAL
{
    public class Message
    {
        public Message(int userId, int chatId, string messageText)
        {
            UserId = userId;
            ChatId = chatId;
            MessageText = messageText;
            CreatedDate = DateTime.Now;

        }

        public int MessageId { get; set; }

        public int ChatId { get; set; } = default!;
        //sender
        public int UserId { get; set; } = default!;

        public string MessageText { get; set; } = default!;

        public DateTime CreatedDate { get; set; } = default!;

    }
}