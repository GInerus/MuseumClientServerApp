using System.Text.Json.Serialization;

namespace MuseumClient.Models
{
    public class MuseumInfoResponse
    {
        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName("data")]
        public MuseumInfoDto? Data { get; set; }
    }

    public class MuseumInfoDto
    {
        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;
    }
}