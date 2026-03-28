using MuseumClient.Models;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows;

namespace MuseumClient.Services
{
    public class AuthService
    {
        private readonly ServerConfig _serverConfig;
        private string _token;

        public AuthService(ServerConfig config)
        {
            _serverConfig = config;
        }

        public string CurrentToken => _token;

        public async Task<bool> RegisterAsync(string userType, string password)
        {
            var url = $"{_serverConfig.Protocol}://{_serverConfig.Host}:{_serverConfig.Port}/api/Session/register";
            var payload = new { userType, password };

            using var client = new HttpClient();
            var response = await client.PostAsJsonAsync(url, payload);
            if (!response.IsSuccessStatusCode) return false;

            var json = await response.Content.ReadFromJsonAsync<UserSession>();
            if (json.status == "ok")
            {
                _token = json.token;
                return true;
            }
            return false;
        }
    }
}