using System;
using System.Text.Json.Serialization;

namespace MuseumClient.Models
{
    public class UpdateBackupSettingsRequest
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
}