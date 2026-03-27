using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using MeseumClient.Config;
using MeseumClient.Models;

namespace MeseumClient.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public ApiService(HttpClient httpClient, ServerConfig config)
        {
            _httpClient = httpClient;
            _baseUrl = $"http://{config.Ip}:{config.Port}/api";
        }

        private async Task<ServerResponse<T>?> GetAsync<T>(string url)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<ServerResponse<T>>($"{_baseUrl}/{url}");
            }
            catch
            {
                return null;
            }
        }

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

        public async Task<ServerResponse<Exhibit[]>?> GetExhibitsAsync()
            => await GetAsync<Exhibit[]>("exhibits");

        public async Task<ServerResponse<Exhibit>?> AddExhibitAsync(Exhibit exhibit)
            => await PostAsync<Exhibit>("exhibits", exhibit);
    }

    public record Exhibit(int Id, string Name, string Description);
}