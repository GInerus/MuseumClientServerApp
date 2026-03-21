using System.Text.Json;
using System.Text.Json.Serialization;

namespace MeseumClient.Models
{

    // Модель для десериализации JSON-ответа

    public class ServerResponse
    {
        [JsonPropertyName("status")]
        public string Status { get; set; } = "error";

        [JsonPropertyName("data")]
        public JsonElement? Data { get; set; }  // Содержит объект с token при успешной регистрации

        [JsonPropertyName("message")]
        public string Message { get; set; } = "";
    }
}
