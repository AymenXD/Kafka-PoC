using Microsoft.EntityFrameworkCore;
using Consumers.Models;

namespace Consumers.Database
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { } 

        public DbSet<KafkaEntry> KafkaEntries { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<KafkaEntry>().ToTable("KafkaEntries");
            base.OnModelCreating(modelBuilder);
        }
    }
}
