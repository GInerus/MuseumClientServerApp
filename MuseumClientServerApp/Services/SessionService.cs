using MuseumServer.Data;
using System;
using System.Linq;

namespace MuseumServer.Services
{
    public class SessionService
    {
        private const string AdminPassword = "qwerty"; // фиксированный пароль

        public string CreateSession(string userType)
        {
            var token = Guid.NewGuid().ToString();
            using var db = new MuseumContext();
            db.Sessions.Add(new Session
            {
                Token = token,
                UserType = userType,
                CreatedAt = DateTime.Now,
                LastAccess = DateTime.Now
            });
            db.SaveChanges();
            return token;
        }

        public bool ValidateSession(string token)
        {
            using var db = new MuseumContext();
            var session = db.Sessions.FirstOrDefault(s => s.Token == token);
            if (session != null)
            {
                session.LastAccess = DateTime.Now; // обновляем время последнего обращения
                db.SaveChanges();
                return true;
            }
            return false;
        }

        public bool ValidateAdminPassword(string password)
        {
            return password == AdminPassword;
        }

        public void CleanupOldSessions(TimeSpan maxAge)
        {
            using var db = new MuseumContext();
            var cutoff = DateTime.Now - maxAge;
            var oldSessions = db.Sessions.Where(s => s.LastAccess < cutoff).ToList();
            if (oldSessions.Count > 0)
            {
                db.Sessions.RemoveRange(oldSessions);
                db.SaveChanges();
            }
        }
    }
}