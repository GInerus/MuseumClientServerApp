using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using MeseumClient.Models;

namespace MeseumClient.Services
{
    public class SessionService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public string? Token { get; private set; }

        public SessionService()
        {
            // читаем конфиг
            if (!File.Exists("appsettings.json"))
                throw new Exception("Файл конфигурации appsettings.json не найден");

            var jsonText = File.ReadAllText("appsettings.json");

            JsonElement configRoot;
            try
            {
                configRoot = JsonSerializer.Deserialize<JsonElement>(jsonText);
            }
            catch (Exception)
            {
                throw new Exception("Не удалось прочитать конфигурацию");
            }

            var serverElement = configRoot.GetProperty("Server");
            var serverIp = serverElement.GetProperty("Ip").GetString()
                ?? throw new Exception("IP сервера не задан в конфиге");
            var serverPort = serverElement.GetProperty("Port").GetInt32();

            _baseUrl = $"http://{serverIp}:{serverPort}/api/Session";
            _httpClient = new HttpClient();
        }

        public async Task<bool> RegisterSessionAsync(string userType, string password)
        {
            var request = new { userType, password };

            var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}/register", request);
            if (!response.IsSuccessStatusCode) return false;

            var result = await response.Content.ReadFromJsonAsync<SessionRegisterResponse>();
            if (result != null && !string.IsNullOrEmpty(result.Token))
            {
                Token = result.Token;
                return true;
            }

            return false;
        }

        public async Task<string?> ValidateTokenAsync()
        {
            if (string.IsNullOrEmpty(Token)) return null;

            var response = await _httpClient.GetAsync($"{_baseUrl}/validate/{Token}");
            if (!response.IsSuccessStatusCode) return null;

            var result = await response.Content.ReadFromJsonAsync<SessionValidateResponse>();

            if (result?.status == "ok")
                return result.userType;

            return null;
        }
    }

    public class SessionRegisterResponse
    {
        public string Token { get; set; } = "";
    }

    public class SessionValidateResponse
    {
        public string status { get; set; } = "";
        public string userType { get; set; } = "";
    }
}