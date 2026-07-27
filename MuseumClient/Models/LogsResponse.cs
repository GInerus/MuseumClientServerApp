using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MuseumClient.Models
{
    public class LogsResponse
    {
        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;
        [JsonPropertyName("data")]
        public List<LogEntryDto> Data { get; set; } = new();
    }

    public class LogEntryDto
    {
        [JsonPropertyName("logId")]
        public int LogId { get; set; }
        [JsonPropertyName("userType")]
        public string UserType { get; set; } = string.Empty;
        [JsonPropertyName("action")]
        public string Action { get; set; } = string.Empty;
        [JsonPropertyName("entityType")]
        public string? EntityType { get; set; }
        [JsonPropertyName("entityName")]
        public string? EntityName { get; set; }
        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; }

        public string FormattedTimestamp => Timestamp.ToLocalTime().ToString("dd.MM.yyyy HH:mm");

        // Готовая фраза для журнала, в духе "admin добавил экспонат 'Старинная монета'"
        public string DisplayText
        {
            get
            {
                var entityLabel = EntityType switch
                {
                    "Exhibit" => "экспонат",
                    "Document" => "статью",
                    "Department" => "отдел",
                    "Image" => "изображение",
                    "Video" => "видео",
                    "MuseumInfo" => "информацию о музее",
                    _ => EntityType
                };

                var name = string.IsNullOrWhiteSpace(EntityName) ? "" : $" \"{EntityName}\"";

                return Action switch
                {
                    "Login" => $"{UserType} вошёл в систему",
                    "ChangePassword" => $"{UserType} сменил пароль администратора",
                    "Create" => $"{UserType} добавил {entityLabel}{name}",
                    "Update" => $"{UserType} изменил {entityLabel}{name}",
                    "Delete" => $"{UserType} удалил {entityLabel}{name}",
                    "SessionCleanup" => $"Системная очистка сессий: {EntityName}",
                    _ => $"{UserType}: {Action} {entityLabel}{name}"
                };
            }
        }
    }
}