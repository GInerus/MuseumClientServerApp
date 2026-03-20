// Ответ

namespace MuseumServer.Models
{
    public class Response
    {
        public string Status { get; set; } = string.Empty; // "ok" / "error"
        public object? Data { get; set; } = null;
        public string Message { get; set; } = string.Empty;
    }
}