using System;
using System.Diagnostics;
using System.Text.Json;
using MeseumClient.Models;

namespace MeseumClient.Services
{
    public class SessionService
    {
        private readonly TcpClientService _tcp;

        public string? ServerIp { get; private set; }
        public string? Token { get; private set; }

        public SessionService(TcpClientService tcp)
        {
            _tcp = tcp;
            ServerIp = tcp != null ? "localhost" : null; // можно брать из конфигурации
        }

        public async Task<bool> RegisterSessionAsync(string userType, string password = "")
        {
            if (ServerIp == null)
            {
                Debug.WriteLine("[ERROR] Сервер не задан.");
                return false;
            }

            var request = new
            {
                action = "REGISTER_SESSION",
                token = (string?)null,
                data = new { userType, password }
            };

            string resp = await _tcp.SendRequestAsync(request);

            try
            {
                var response = JsonSerializer.Deserialize<ServerResponse>(resp);
                if (response?.Status?.ToLower() != "ok")
                {
                    Debug.WriteLine($"[ERROR] Сервер вернул ошибку: {response?.Message ?? "(no message)"}");
                    return false;
                }

                Token = response.Data?.GetProperty("token").GetString();
                Debug.WriteLine(Token != null ? $"[DEBUG] Токен получен: {Token}" : "[ERROR] Токен не получен");
                return Token != null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERROR] Ошибка парсинга JSON: {ex.Message}");
                return false;
            }
        }

        public async Task<ServerResponse?> SendCommandAsync(string action, object? data = null)
        {
            if (ServerIp == null || Token == null)
            {
                Debug.WriteLine("[ERROR] Сессия не инициализирована.");
                return new ServerResponse { Status = "error", Message = "NO_SESSION" };
            }

            var request = new { action, token = Token, data };
            string resp = await _tcp.SendRequestAsync(request);

            try
            {
                return JsonSerializer.Deserialize<ServerResponse>(resp);
            }
            catch
            {
                return new ServerResponse { Status = "error", Message = "INVALID_JSON" };
            }
        }
    }
}