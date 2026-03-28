using MuseumClient.Models;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace MuseumClient.Services
{
    public class AuthService
    {
        private static AuthService? _instance;

        // Singleton — один экземпляр на всё приложение
        public static AuthService Instance(ServerConfig? config = null)
        {
            if (_instance == null)
            {
                if (config == null) throw new System.Exception("AuthService не инициализирован!");
                _instance = new AuthService(config);
            }
            return _instance;
        }

        private readonly ServerConfig _serverConfig;
        private string _token;

        private AuthService(ServerConfig config)
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
            if (json?.status == "ok")
            {
                _token = json.token; // токен хранится в этом экземпляре
                return true;
            }
            return false;
        }
    }
}