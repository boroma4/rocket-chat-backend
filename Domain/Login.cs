namespace DAL
{
    public class Login
    {
        public int LoginId { get; set; }

        public string? UserName { get; set; }
        public string Email { get; set; } = default!;

        public string Password { get; set; } = default!;
    }
}