
using M183.Models;
using M183.Models.AuditModels;
using Microsoft.EntityFrameworkCore;

namespace M183.Data
{
    public class NewsAppContext : DbContext
    {
        public NewsAppContext(DbContextOptions<NewsAppContext> options) : base(options) { }
        public DbSet<News> News { get; set; }
        public DbSet<NewsAudit> NewsAudit { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            new NewsAppInitializer(modelBuilder).Seed();

            modelBuilder.Entity<News>()
                .ToTable(tb => tb.HasTrigger("news_insert"))
                .ToTable(tb => tb.HasTrigger("news_update"))
                .ToTable(tb => tb.HasTrigger("news_delete"));
        }
    }
}
