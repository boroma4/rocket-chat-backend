using System.Collections;
using System.Collections.Generic;

namespace DAL
{
    public class User
    {
        public int UserId { get; set; }

        public string UserName { get; set; } = default!;

        public Login Login { get; set; } = default!;

        public bool IsOnline { get; set; } = default!;

        public ICollection<ChatUser>? UserChats { get; set; }
    }
}