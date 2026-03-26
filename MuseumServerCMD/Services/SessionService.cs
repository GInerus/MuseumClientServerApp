using MuseumServer.Data;
using System;
using System.Linq;

namespace MuseumServer.Services
{
    public class SessionService
    {
        // ⚠️ лучше вынести в конфиг (appsettings.json)
        private readonly string _adminPassword;

        // TTL сессии (например 10 минут)
        private readonly TimeSpan _sessionLifetime = TimeSpan.FromMinutes(10);

        public SessionService(string adminPassword = "qwerty")
        {
            _adminPassword = adminPassword;
        }

        // 🔑 Создание сессии
        public string CreateSession(string userType)
        {
            var token = Guid.NewGuid().ToString();

            try
            {
                using var db = new MuseumContext();

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
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка создания сессии: {ex}");
                return string.Empty;
            }
        }

        // ✅ Проверка сессии
        public bool ValidateSession(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return false;

            try
            {
                using var db = new MuseumContext();

                var session = db.Sessions.SingleOrDefault(s => s.Token == token);
                if (session == null)
                    return false;

                // ⏱ Проверка срока жизни
                if (session.LastAccess < DateTime.UtcNow - _sessionLifetime)
                    return false;

                // 🔄 Обновляем время активности
                session.LastAccess = DateTime.UtcNow;
                db.SaveChanges();

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка валидации сессии: {ex}");
                return false;
            }
        }

        // 🔐 Проверка админ-пароля
        public bool ValidateAdminPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
                return false;

            return password == _adminPassword;
        }

        // 🧹 Очистка старых сессий
        public void CleanupOldSessions()
        {
            try
            {
                using var db = new MuseumContext();

                var cutoff = DateTime.UtcNow - _sessionLifetime;

                var oldSessions = db.Sessions
                    .Where(s => s.LastAccess < cutoff)
                    .ToList();

                if (oldSessions.Count == 0)
                    return;

                db.Sessions.RemoveRange(oldSessions);
                db.SaveChanges();

                Console.WriteLine($"Удалено сессий: {oldSessions.Count}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка очистки сессий: {ex}");
            }
        }
    }
}