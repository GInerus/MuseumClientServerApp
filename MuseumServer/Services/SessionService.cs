using MuseumServer.Data;
using MuseumServer.Models;
using Microsoft.EntityFrameworkCore;

namespace MuseumServer.Services
{
    public class SessionService
    {
        private readonly IDbContextFactory<MuseumContext> _dbFactory;
        private readonly string _adminPassword;
        private readonly TimeSpan _sessionLifetime = TimeSpan.FromMinutes(10);

        public SessionService(IDbContextFactory<MuseumContext> dbFactory)
        {
            _dbFactory = dbFactory;

            var path = Path.Combine(AppContext.BaseDirectory, "Config", "password.txt");
            if (!File.Exists(path))
                throw new FileNotFoundException("Файл с паролем администратора не найден", path);

            _adminPassword = File.ReadAllText(path).Trim();
        }

        public string CreateSession(string userType)
        {
            var token = Guid.NewGuid().ToString();
            using var db = _dbFactory.CreateDbContext();
            db.Sessions.Add(new Session
            {
                Token = token,
                UserType = userType,
                CreatedAt = DateTime.UtcNow,
                LastAccess = DateTime.UtcNow
            });
            db.SaveChanges();
            return token;
        }

        public bool ValidateSession(string token)
        {
            using var db = _dbFactory.CreateDbContext();
            var session = db.Sessions.FirstOrDefault(s => s.Token == token);
            if (session != null)
            {
                session.LastAccess = DateTime.UtcNow;
                db.SaveChanges();
                return true;
            }
            return false;
        }

        public bool ValidateAdminPassword(string password) => password == _adminPassword;

        public void CleanupOldSessions()
        {
            using var db = _dbFactory.CreateDbContext();
            var cutoff = DateTime.UtcNow - _sessionLifetime;
            var oldSessions = db.Sessions.Where(s => s.LastAccess < cutoff).ToList();
            if (oldSessions.Count > 0)
            {
                db.Sessions.RemoveRange(oldSessions);
                db.SaveChanges();
                Console.WriteLine($"Удалено сессий: {oldSessions.Count}");
            }
        }
    }
}