// Ответ

using System.Text.Json.Serialization;

namespace MuseumServer.Models
{
    public class Response
    {
        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("data")]
        public object Data { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }
    }
}