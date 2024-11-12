using BloggingPlatform.models;
using Microsoft.EntityFrameworkCore;

namespace BloggingPlatform.config
{

    public class DataContextEF : DbContext
    {
        private readonly IConfiguration _config;

        public DataContextEF(IConfiguration config)
        {
            _config = config;
        }

        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<Blog> Blogs { get; set; }
        public virtual DbSet<Otp> Otps { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder
                    .UseSqlServer(_config.GetConnectionString("DefaultConnection"),
                        optionsBuilder => optionsBuilder.EnableRetryOnFailure());
            }
        }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("BloggingPlatform");

            modelBuilder.Entity<User>()
                .ToTable("Users", "BloggingPlatform")
                .HasKey(u => u.UserId);

            modelBuilder.Entity<Blog>()
                .ToTable("Blogs", "BloggingPlatform")
                .HasKey(b => b.BlogId); // Set the primary key

            modelBuilder.Entity<Blog>()
                .HasOne(b => b.Author) // Configure the relationship
                .WithMany() // If User has many blogs, this is sufficient
                .HasForeignKey(b => b.AuthorId) // Set AuthorId as the foreign key
                .OnDelete(DeleteBehavior.Cascade); // Configure delete behavior

            modelBuilder.Entity<Otp>()
                .ToTable("Otps","BloggingPlatform")
                .HasKey(o => o.OtpId);

            modelBuilder.Entity<Otp>()
                .HasOne(o => o.User)
                .WithOne()
                .HasForeignKey<Otp>(o => o.UserId)
                .OnDelete(DeleteBehavior.Cascade);


        }

    }


}