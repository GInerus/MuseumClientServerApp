using Microsoft.Extensions.Hosting;

namespace MuseumServer.Services
{
    public class BackupSchedulerService : BackgroundService
    {
        private readonly BackupService _backupService;

        public BackupSchedulerService(BackupService backupService)
        {
            _backupService = backupService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await _backupService.RunScheduledTickAsync(stoppingToken);
                }
                catch
                {
                    // scheduler не должен ронять приложение
                }

                try
                {
                    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                }
                catch (TaskCanceledException)
                {
                    break;
                }
            }
        }
    }
}