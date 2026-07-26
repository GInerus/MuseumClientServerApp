using Microsoft.EntityFrameworkCore;
using MuseumServer.Data;
using MuseumServer.Models;

namespace MuseumServer.Services
{
    public class LoggingService
    {
        private readonly IDbContextFactory<MuseumContext> _dbFactory;

        public LoggingService(IDbContextFactory<MuseumContext> dbFactory)
        {
            _dbFactory = dbFactory;
        }

        public async Task LogAsync(
            string userType,
            string action,
            string? entityType = null,
            string? entityName = null)
        {
            try
            {
                using var db = _dbFactory.CreateDbContext();

                db.Logs.Add(new LogEntry
                {
                    UserType = userType,
                    Action = action,
                    EntityType = entityType,
                    EntityName = entityName,
                    Timestamp = DateTime.UtcNow
                });

                await db.SaveChangesAsync();
            }
            catch
            {
                // логирование не должно ронять основную операцию
            }
        }
    }
}