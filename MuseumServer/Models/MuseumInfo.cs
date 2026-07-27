using System.ComponentModel.DataAnnotations;

namespace MuseumServer.Models
{
    public class MuseumInfo
    {
        public int MuseumInfoId { get; set; }
        public string? Description { get; set; }
        public string AdminPasswordHash { get; set; } = string.Empty;

        public int BackupFullDayOfWeek { get; set; } = 0;          // 0 = Sunday - Воскресенье
        public TimeSpan BackupFullTime { get; set; } = new(3, 0, 0);
        public TimeSpan BackupDifferentialTime { get; set; } = new(3, 0, 0);
        public int BackupRetentionDays { get; set; } = 30;
    }
}