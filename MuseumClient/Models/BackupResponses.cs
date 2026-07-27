using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MuseumClient.Models
{
    public class BackupListResponse
    {
        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName("data")]
        public List<BackupPointDto> Data { get; set; } = new();
    }

    public class BackupSettingsResponse
    {
        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName("data")]
        public BackupSettingsDto? Data { get; set; }
    }

    public class BackupOperationResponse
    {
        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName("message")]
        public string? Message { get; set; }

        [JsonPropertyName("data")]
        public BackupOperationResultDto? Data { get; set; }
    }
}