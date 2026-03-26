using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace MeseumClient.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public ApiService(string serverIp, int port)
        {
            _baseUrl = $"http://{serverIp}:{port}/api"; // базовый URL API
            _httpClient = new HttpClient();
        }

        // Пример: получить список экспонатов
        public async Task<Exhibit[]> GetExhibitsAsync()
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<Exhibit[]>($"{_baseUrl}/exhibits");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при запросе API: {ex.Message}");
                return Array.Empty<Exhibit>();
            }
        }

        // Пример: отправка данных на сервер
        public async Task<bool> AddExhibitAsync(Exhibit exhibit)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}/exhibits", exhibit);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
    }

    // Пример модели данных (DTO)
    public class Exhibit
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
    }
}