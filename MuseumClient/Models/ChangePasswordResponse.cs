using System.Text.Json.Serialization;

namespace MuseumClient.Models
{
    public class ChangePasswordResponse
    {
        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;
        [JsonPropertyName("message")]
        public string? Message { get; set; }
    }
}   