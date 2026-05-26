using Microsoft.EntityFrameworkCore;
using EnglishCore.Models;

namespace EnglishCore.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }

        public DbSet<Sentence> Sentences { get; set; }
        public DbSet<Word> Words { get; set; }
        public DbSet<AppUser> AppUsers { get; set; }
        public DbSet<UserSentenceProgress> UserSentenceProgresses { get; set; }
        public DbSet<UserWordProgress> UserWordProgresses { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Aynı Username ile iki kullanıcı kaydı oluşmasını DB seviyesinde engeller
            modelBuilder.Entity<AppUser>()
                .HasIndex(u => u.Username)
                .IsUnique();

            // Aynı kullanıcı için aynı kelimeye birden fazla ilerleme kaydı açılmasını engeller
            modelBuilder.Entity<UserWordProgress>()
                .HasIndex(p => new { p.AppUserId, p.WordId })
                .IsUnique();

            // Aynı kullanıcı için aynı cümleye birden fazla ilerleme kaydı açılmasını engeller
            modelBuilder.Entity<UserSentenceProgress>()
                .HasIndex(p => new { p.AppUserId, p.SentenceId })
                .IsUnique();
        }
    }
}
