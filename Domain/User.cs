using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DAL
{
    public class User
    {
        public int UserId { get; set; }

        [MinLength(2)]
        [MaxLength(64)]
        public string UserName { get; set; } = default!;

        public Login Login { get; set; } = default!;

        public ICollection<ChatUser>? UserChats { get; set; }
    }
}