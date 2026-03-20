// Запрос

using System.Text.Json;

namespace MuseumServer.Models
{
    public class Request
    {
        public string Action { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public JsonElement? Data { get; set; }
    }
}