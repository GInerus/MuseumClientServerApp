using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using MeseumClient.Models;

namespace MeseumClient.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        // HttpClient теперь передаём извне, чтобы избежать утечек сокетов
        public ApiService(HttpClient httpClient, string serverIp, int port)
        {
            _baseUrl = $"http://{serverIp}:{port}/api";
            _httpClient = httpClient;
        }

        // Универсальный метод для GET запросов
        private async Task<ServerResponse<T>?> GetAsync<T>(string url)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<ServerResponse<T>>($"{_baseUrl}/{url}");
            }
            catch
            {
                // Можно добавить логирование здесь
                return null;
            }
        }

        // Универсальный метод для POST запросов
        private async Task<ServerResponse<T>?> PostAsync<T>(string url, object payload)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}/{url}", payload);
                return await response.Content.ReadFromJsonAsync<ServerResponse<T>>();
            }
            catch
            {
                return null;
            }
        }

        // Получение экспонатов
        public async Task<ServerResponse<Exhibit[]>?> GetExhibitsAsync()
        {
            return await GetAsync<Exhibit[]>("exhibits");
        }

        // Добавление экспоната
        public async Task<ServerResponse<Exhibit>?> AddExhibitAsync(Exhibit exhibit)
        {
            return await PostAsync<Exhibit>("exhibits", exhibit);
        }
    }

    // Модель экспоната — теперь record для иммутабельности
    public record Exhibit(int Id, string Name, string Description);
}