using System;
using System.Text.Json.Serialization;

namespace MuseumClient.Models
{
    public class BackupPointDto
    {
        [JsonPropertyName("baseName")]
        public string BaseName { get; set; } = string.Empty;

        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("backupType")]
        public string BackupType { get; set; } = string.Empty;

        [JsonPropertyName("databaseFileName")]
        public string? DatabaseFileName { get; set; }

        [JsonPropertyName("hasDatabaseBackup")]
        public bool HasDatabaseBackup { get; set; }

        [JsonPropertyName("mediaFileName")]
        public string? MediaFileName { get; set; }

        [JsonPropertyName("hasMediaArchive")]
        public bool HasMediaArchive { get; set; }

        [JsonPropertyName("databaseSizeBytes")]
        public long DatabaseSizeBytes { get; set; }

        [JsonPropertyName("mediaSizeBytes")]
        public long MediaSizeBytes { get; set; }

        public string BackupTypeDisplay =>
            BackupType == "Full"
                ? "Полный"
                : "Разностный";

        public string CreatedAtDisplay =>
            CreatedAt.ToString("dd.MM.yyyy HH:mm:ss");

        public string DatabaseSizeDisplay => FormatSize(DatabaseSizeBytes);

        public string MediaSizeDisplay => FormatSize(MediaSizeBytes);

        private static string FormatSize(long bytes)
        {
            string[] suffixes = { "Б", "КБ", "МБ", "ГБ" };

            double size = bytes;
            int index = 0;

            while (size >= 1024 && index < suffixes.Length - 1)
            {
                size /= 1024;
                index++;
            }

            return $"{size:0.##} {suffixes[index]}";
        }
    }

    public class BackupSettingsDto
    {
        [JsonPropertyName("backupFullDayOfWeek")]
        public int BackupFullDayOfWeek { get; set; }

        [JsonPropertyName("backupFullTime")]
        public TimeSpan BackupFullTime { get; set; }

        [JsonPropertyName("backupDifferentialTime")]
        public TimeSpan BackupDifferentialTime { get; set; }

        [JsonPropertyName("backupRetentionDays")]
        public int BackupRetentionDays { get; set; }
    }

    public class BackupOperationResultDto
    {
        [JsonPropertyName("baseName")]
        public string BaseName { get; set; } = string.Empty;

        [JsonPropertyName("mediaArchiveCreated")]
        public bool MediaArchiveCreated { get; set; }

        [JsonPropertyName("warning")]
        public string? Warning { get; set; }
    }
}