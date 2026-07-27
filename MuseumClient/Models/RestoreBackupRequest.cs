using System.Text.Json.Serialization;

namespace MuseumClient.Models
{
    public class RestoreBackupRequest
    {
        [JsonPropertyName("baseName")]
        public string BaseName { get; set; } = string.Empty;

        [JsonPropertyName("restoreMedia")]
        public bool RestoreMedia { get; set; }
    }
}