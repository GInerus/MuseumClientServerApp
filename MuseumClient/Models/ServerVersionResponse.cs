using System.Text.Json.Serialization;

namespace MuseumClient.Models
{
    public class ServerVersionResponse
    {
        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName("data")]
        public ServerVersionDto? Data { get; set; }
    }

    public class ServerVersionDto
    {
        [JsonPropertyName("version")]
        public string Version { get; set; } = string.Empty;
    }
}