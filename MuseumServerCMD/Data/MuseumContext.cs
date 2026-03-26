using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace MuseumServer.Data
{
    public class MuseumContext : Microsoft.EntityFrameworkCore.DbContext
    {
        public DbSet<Session> Sessions { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Exhibit> Exhibits { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Используем локальный SQL Server
            optionsBuilder.UseSqlServer(@"Data Source=GERMAN-PC;Initial Catalog=MuseumDB;Integrated Security=True;Trust Server Certificate=True");
            //optionsBuilder.UseSqlServer(@"Data Source=192.168.0.150;Initial Catalog=MuseumDB;User ID=sa;Password=schoolpas68G#;Trust Server Certificate=True");

        }
    }
}