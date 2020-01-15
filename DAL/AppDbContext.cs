using System;
using DAL;
using Microsoft.EntityFrameworkCore;

namespace Domain
{
    public class AppDbContext : DbContext
    {
        public DbSet<Chat> Chats { get; set; } = default!;

        public DbSet<Login> Logins { get; set; } = default!;

        public DbSet<Message> Messages { get; set; } = default!;

        public DbSet<User> Users { get; set; } = default!;

        public AppDbContext(DbContextOptions options) : base(options)
        {

        }
    }
}