using System;
using System.Collections;
using System.Collections.Generic;

namespace DAL
{
    public class Chat
    {
        public int ChatId { get; set; }

        public ICollection<ChatUser> Users { get; set; } = default!;

        public ICollection<Message>? Messages { get; set; }
        
    }
}