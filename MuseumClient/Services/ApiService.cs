using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
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

        public async Task<T> SendRequestAsync<T>(string endpoint, object payload)
        {
            using var client = new HttpClient();

            if (!string.IsNullOrEmpty(_authService.CurrentToken))
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _authService.CurrentToken);

            var url = $"{_serverConfig.Protocol}://{_serverConfig.Host}:{_serverConfig.Port}/api/{endpoint}";
            var response = await client.PostAsJsonAsync(url, payload);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<T>();
        }
    }
}