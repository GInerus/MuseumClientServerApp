using Microsoft.EntityFrameworkCore;
using MuseumServer.Models;

namespace MuseumServer.Data
{
    public class MuseumContext : DbContext
    {
        public MuseumContext(DbContextOptions<MuseumContext> options) : base(options) { }

        public DbSet<Session> Sessions { get; set; } = null!;
        public DbSet<Exhibit> Exhibits { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Document> Documents { get; set; }
        public DbSet<MediaFile> MediaFiles { get; set; }
        public DbSet<MuseumInfo> MuseumInfo { get; set; } = null!;
        public DbSet<LogEntry> Logs { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Session>(entity =>
            {
                entity.HasKey(e => e.SessionId);
                entity.Property(e => e.Token).IsRequired();
                entity.Property(e => e.UserType).IsRequired();
                entity.Property(e => e.CreatedAt).IsRequired();
                entity.Property(e => e.LastAccess).IsRequired();
            });

            modelBuilder.Entity<MuseumInfo>(entity =>
            {
                entity.HasKey(e => e.MuseumInfoId);

                entity.Property(e => e.Description);
                entity.Property(e => e.AdminPasswordHash).IsRequired();

                entity.Property(e => e.BackupFullDayOfWeek).IsRequired().HasDefaultValue(0);
                entity.Property(e => e.BackupFullTime)
                    .HasColumnType("time")
                    .IsRequired()
                    .HasDefaultValue(new TimeSpan(3, 0, 0));

                entity.Property(e => e.BackupDifferentialTime)
                    .HasColumnType("time")
                    .IsRequired()
                    .HasDefaultValue(new TimeSpan(3, 0, 0));

                entity.Property(e => e.BackupRetentionDays).IsRequired().HasDefaultValue(30);
            });
        }
    }
}