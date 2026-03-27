using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using MeseumClient.Config;

namespace MeseumClient.Services
{
    public class SessionService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public string? Token { get; private set; }
        public string? UserType { get; private set; }

        public SessionService(HttpClient httpClient, ServerConfig config)
        {
            _httpClient = httpClient;
            _baseUrl = $"http://{config.Ip}:{config.Port}/api/Session";
        }

        // Регистрация сессии
        public async Task<SessionRegisterResponse?> RegisterSessionAsync(string userType, string password)
        {
            try
            {
                var request = new { userType, password };
                var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}/register", request);
                var result = await response.Content.ReadFromJsonAsync<SessionRegisterResponse>();

                if (result?.IsSuccess == true)
                {
                    Token = result.Token;
                    UserType = result.UserType;
                }

                return result;
            }
            catch
            {
                return null;
            }
        }

        // Проверка токена
        public async Task<SessionValidateResponse?> ValidateTokenAsync()
        {
            if (string.IsNullOrEmpty(Token)) return null;

            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/validate/{Token}");
                if (!response.IsSuccessStatusCode) return null;

                var result = await response.Content.ReadFromJsonAsync<SessionValidateResponse>();
                return result;
            }
            catch
            {
                return null;
            }
        }

        // Метод для ручной установки токена (если нужно)
        public void SetToken(string token) => Token = token;
    }

    // Модель для десериализации ответа регистрации
    public class SessionRegisterResponse
    {
        [JsonPropertyName("status")]
        public string Status { get; set; } = "";

        [JsonPropertyName("token")]
        public string Token { get; set; } = "";

        [JsonPropertyName("userType")]
        public string UserType { get; set; } = "";

        public bool IsSuccess => Status.Equals("ok", System.StringComparison.OrdinalIgnoreCase);
    }

    // Модель для десериализации проверки токена
    public class SessionValidateResponse
    {
        [JsonPropertyName("status")]
        public string Status { get; set; } = "";

        [JsonPropertyName("userType")]
        public string UserType { get; set; } = "";

        public bool IsSuccess => Status.Equals("ok", System.StringComparison.OrdinalIgnoreCase);
    }
}