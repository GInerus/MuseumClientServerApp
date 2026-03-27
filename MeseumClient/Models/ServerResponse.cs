using System.Text.Json;
using System.Text.Json.Serialization;

namespace MeseumClient.Models
{

    // Модель для десериализации JSON-ответа
    // Старый
    public class ServerResponse
    {
        [JsonPropertyName("status")]
        public string Status { get; set; } = "error";

        [JsonPropertyName("data")]
        public JsonElement? Data { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; } = "";
    }

    // Новый вариант с дженериком
    public class ServerResponse<T>
    {
        [JsonPropertyName("status")]
        public string Status { get; set; } = "error";

        [JsonPropertyName("data")]
        public T? Data { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; } = "";

        public bool IsSuccess => Status.Equals("ok", StringComparison.OrdinalIgnoreCase);
    }
}
