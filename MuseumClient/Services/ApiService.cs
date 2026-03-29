using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows;
using MuseumClient.Models;

namespace MuseumClient.Services
{
    public class ApiService
    {
        private readonly ServerConfig _serverConfig;
        private readonly AuthService _authService;

        public ApiService(ServerConfig config, AuthService authService)
        {
            _serverConfig = config;
            _authService = authService;
        }

        private HttpClient CreateClient()
        {
            var client = new HttpClient();
            var token = _authService.CurrentToken;

            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Add("token", token);
            }
            return client;
        }

        // Универсальный GET
        public async Task<T> GetAsync<T>(string endpoint)
        {
            using var client = CreateClient();
            var url = $"{_serverConfig.Protocol}://{_serverConfig.Host}:{_serverConfig.Port}/api/{endpoint}";
            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<T>();
        }

        // Универсальный POST
        public async Task<T> PostAsync<T>(string endpoint, object payload)
        {
            using var client = CreateClient();
            var url = $"{_serverConfig.Protocol}://{_serverConfig.Host}:{_serverConfig.Port}/api/{endpoint}";
            var response = await client.PostAsJsonAsync(url, payload);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<T>();
        }

        // Универсальный PUT
        public async Task<T> PutAsync<T>(string endpoint, object payload)
        {
            using var client = CreateClient();
            var url = $"{_serverConfig.Protocol}://{_serverConfig.Host}:{_serverConfig.Port}/api/{endpoint}";
            var response = await client.PutAsJsonAsync(url, payload);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<T>();
        }

        // Универсальный DELETE
        public async Task<T> DeleteAsync<T>(string endpoint)
        {
            using var client = CreateClient();
            var url = $"{_serverConfig.Protocol}://{_serverConfig.Host}:{_serverConfig.Port}/api/{endpoint}";
            var response = await client.DeleteAsync(url);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<T>();
        }
    }
}