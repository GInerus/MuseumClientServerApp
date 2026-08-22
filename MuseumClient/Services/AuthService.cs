using MuseumClient.Models;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;

namespace MuseumClient.Services
{
    public class AuthService
    {
        private static AuthService? _instance;

        public event Action? AuthChanged;

        public static AuthService Instance(ServerConfig? config = null)
        {
            if (_instance == null)
            {
                if (config == null)
                    throw new Exception("AuthService не инициализирован!");

                _instance = new AuthService(config);
            }

            return _instance;
        }

        private readonly ServerConfig _serverConfig;

        private string _token;
        private string _baseUrl;
        private string _userType;

        private readonly HttpClient _client;

        private AuthService(ServerConfig config)
        {
            _serverConfig = config;

            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback =
                    HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            };

            _client = new HttpClient(handler)
            {
                Timeout = TimeSpan.FromSeconds(30)  // ← было 2
            };
        }

        public string CurrentToken => _token;
        public string BaseUrl => _baseUrl;

        public string CurrentUserType => _userType;

        public bool IsAdmin => _userType == "admin";
        public bool IsGuest => _userType == "guest";

        private async Task<string> GetWorkingUrlAsync()
        {
            try
            {
                // Отдельный клиент с коротким таймаутом для проверки локалки
                using var localHandler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback =
                        HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                };
                using var localClient = new HttpClient(localHandler)
                {
                    Timeout = TimeSpan.FromSeconds(3)  // ← короткий таймаут, чтобы не висеть
                };

                var localTestUrl = $"{_serverConfig.LocalUrl}/api/health";
                var response = await localClient.GetAsync(localTestUrl);

                System.Diagnostics.Debug.WriteLine("LOCAL TEST: " + localTestUrl);

                if (response.IsSuccessStatusCode)
                    return _serverConfig.LocalUrl;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("LOCAL FAIL: " + ex.Message);
            }

            System.Diagnostics.Debug.WriteLine("USING REMOTE: " + _serverConfig.RemoteUrl);
            return _serverConfig.RemoteUrl;
        }

        public async Task<AuthResult> RegisterAsync(string userType, string password)
        {
            _baseUrl = await GetWorkingUrlAsync();

            var url = $"{_baseUrl}/api/Session/register";

            System.Diagnostics.Debug.WriteLine("REGISTER URL: " + url);

            var payload = new
            {
                userType,
                password
            };

            try
            {
                var response = await _client.PostAsJsonAsync(url, payload);

                var body = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine("RESPONSE: " + body);

                if (!response.IsSuccessStatusCode)
                    return AuthResult.InvalidCredentials;

                // ИЗМЕНЕНО: парсим из уже прочитанной строки, а не из потока повторно
                var json = System.Text.Json.JsonSerializer.Deserialize<UserSession>(body);

                if (json?.status == "ok")
                {
                    _token = json.token;
                    _userType = json.userType;
                    AuthChanged?.Invoke();
                    return AuthResult.Success;
                }

                return AuthResult.InvalidCredentials;
            }
            catch (HttpRequestException ex)
            {
                System.Diagnostics.Debug.WriteLine("REGISTER ERROR: " + ex);
                return AuthResult.ServerUnavailable;
            }
            catch (TaskCanceledException ex)
            {
                System.Diagnostics.Debug.WriteLine("TIMEOUT: " + ex);
                return AuthResult.ServerUnavailable;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("UNEXPECTED ERROR: " + ex);
                return AuthResult.ServerUnavailable;
            }
        }

        public void Logout()
        {
            _token = null;
            _userType = null;

            AuthChanged?.Invoke();
        }

        public void UpdateServerConfig(ServerConfig config)
        {
            _serverConfig.LocalUrl = config.LocalUrl;
            _serverConfig.RemoteUrl = config.RemoteUrl;
        }
    }
}