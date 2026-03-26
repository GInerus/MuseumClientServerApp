using Microsoft.Extensions.Hosting;

namespace MuseumServer.Services
{
    public class SessionCleanupService : BackgroundService
    {
        private readonly SessionService _sessionService;

        public SessionCleanupService(SessionService sessionService)
        {
            _sessionService = sessionService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _sessionService.CleanupOldSessions();
                await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken);
            }
        }
    }
}