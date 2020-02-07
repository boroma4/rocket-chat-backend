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
        
        public DbSet<ChatUser> ChatUsers { get; set; } = default!;

        public DbSet<MediaType> MediaTypes { get; set; } = default!;

        public DbSet<MessageMedia> MessageMedias { get; set; } = default!;

        public DbSet<NotificationSettings> NotificationSettings { get; set; } = default!;


        public AppDbContext(DbContextOptions options) : base(options)
        {

        }
    }
}