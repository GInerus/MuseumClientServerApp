namespace MuseumServer.DTOs
{
    public class BackupSettingsDto
    {
        public int BackupFullDayOfWeek { get; set; } = 0;              // 0 = Sunday
        public TimeSpan BackupFullTime { get; set; } = new(3, 0, 0);
        public TimeSpan BackupDifferentialTime { get; set; } = new(3, 0, 0);
        public int BackupRetentionDays { get; set; } = 30;
    }

    public class UpdateBackupSettingsRequest : BackupSettingsDto
    {
    }

    public class BackupPointDto
    {
        public string BaseName { get; set; } = string.Empty;           // backup_yyyyMMdd_HHmmss
        public DateTime CreatedAt { get; set; }
        public string BackupType { get; set; } = string.Empty;         // Full / Differential

        public string DatabaseFileName { get; set; } = string.Empty;
        public bool HasDatabaseBackup { get; set; }

        public string? MediaFileName { get; set; }
        public bool HasMediaArchive { get; set; }

        public long DatabaseSizeBytes { get; set; }
        public long? MediaSizeBytes { get; set; }
    }

    public class BackupOperationResultDto
    {
        public string BaseName { get; set; } = string.Empty;
        public bool MediaArchiveCreated { get; set; }
        public string? Warning { get; set; }
    }

    public class RestoreBackupRequest
    {
        public string BaseName { get; set; } = string.Empty;
        public bool RestoreMedia { get; set; } = true;
    }
}