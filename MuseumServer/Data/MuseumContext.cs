using Microsoft.EntityFrameworkCore;
using MuseumServerApi.Models;

namespace MuseumServer.Data
{
    public class MuseumContext : DbContext
    {
        public MuseumContext(DbContextOptions<MuseumContext> options) : base(options) { }

        public DbSet<Session> Sessions { get; set; } = null!;

        // Для будущих расширений:
        // public DbSet<Department> Departments { get; set; } 
        // public DbSet<Exhibit> Exhibits { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Конфигурация Session
            modelBuilder.Entity<Session>(entity =>
            {
                entity.HasKey(e => e.SessionId);
                entity.Property(e => e.Token).IsRequired();
                entity.Property(e => e.UserType).IsRequired();
                entity.Property(e => e.CreatedAt).IsRequired();
                entity.Property(e => e.LastAccess).IsRequired();
            });
        }
    }
}