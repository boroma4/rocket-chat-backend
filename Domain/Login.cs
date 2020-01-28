using System.ComponentModel.DataAnnotations;

namespace DAL
{
    public class Login
    {
        public int LoginId { get; set; }


        [MinLength(2)]
        [MaxLength(64)]
        public string UserName { get; set; } = default!;
        [MinLength(5)]
        [MaxLength(64)]
        public string Email { get; set; } = default!;

        public string Password { get; set; } = default!;
        
    }
}