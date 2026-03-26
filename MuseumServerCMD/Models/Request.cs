// Запрос

using System.Text.Json;
using System.Text.Json.Serialization;

namespace MuseumServer.Models
{
    public class Request
    {
        [JsonPropertyName("action")]
        public string Action { get; set; }

        [JsonPropertyName("token")]
        public string Token { get; set; }

        [JsonPropertyName("data")]
        public JsonElement? Data { get; set; }
    }
}