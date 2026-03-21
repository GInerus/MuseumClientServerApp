using System;
using System.Diagnostics;
using System.Text.Json;
using MeseumClient.Models;

namespace MeseumClient.Services
{
    public class SessionService
    {
        private readonly NetworkDiscoveryService _discovery;
        private readonly TcpClientService _tcp;

        public string? ServerIp { get; private set; }
        public string? Token { get; private set; }

        public SessionService(NetworkDiscoveryService discovery, TcpClientService tcp)
        {
            _discovery = discovery;
            _tcp = tcp;
        }

        // Поиск сервера в сети
        public async Task<bool> DiscoverServerAsync()
        {
            Debug.WriteLine("[DEBUG] Поиск сервера в сети...");
            ServerIp = await _discovery.DiscoverServerAsync();
            Debug.WriteLine(ServerIp != null
                ? $"[DEBUG] Сервер найден: {ServerIp}"
                : "[DEBUG] Сервер не найден");
            return ServerIp != null;
        }

        // Регистрация сессии (guest или admin)
        public async Task<bool> RegisterSessionAsync(string userType, string password = "")
        {
            if (ServerIp == null)
            {
                Debug.WriteLine("[ERROR] Сервер не найден. Регистрация сессии невозможна.");
                return false;
            }

            var request = new
            {
                action = "REGISTER_SESSION",
                token = (string?)null,
                data = new
                {
                    userType,
                    password
                }
            };

            Debug.WriteLine($"[DEBUG] Отправка запроса на регистрацию сессии: userType={userType}, password='{password}'");
            string resp = await _tcp.SendRequestAsync(ServerIp, request);

            try
            {
                var response = JsonSerializer.Deserialize<ServerResponse>(resp);
                if (response == null)
                {
                    Debug.WriteLine("[ERROR] Ответ сервера пустой или некорректный JSON");
                    return false;
                }

                // Проверяем только Status
                if (!string.Equals(response.Status, "ok", StringComparison.OrdinalIgnoreCase))
                {
                    string msg = string.IsNullOrEmpty(response.Message) ? "(no message)" : response.Message;
                    Debug.WriteLine($"[ERROR] Сервер вернул ошибку: {msg}");
                    return false;
                }

                // Получаем токен
                Token = response.Data?.GetProperty("token").GetString();
                Debug.WriteLine(Token != null
                    ? $"[DEBUG] Сессия создана. Токен: {Token}"
                    : "[ERROR] Токен не получен");
                return Token != null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERROR] Ошибка парсинга JSON: {ex.Message}");
                return false;
            }
        }

        // Общие команды к серверу
        public async Task<ServerResponse?> SendCommandAsync(string action, object? data = null)
        {
            if (ServerIp == null || Token == null)
            {
                Debug.WriteLine("[ERROR] Сессия не инициализирована. Команда не отправлена.");
                return new ServerResponse { Status = "error", Message = "NO_SESSION" };
            }

            var request = new
            {
                action,
                token = Token,
                data
            };

            Debug.WriteLine($"[DEBUG] Отправка команды '{action}' с данными: {JsonSerializer.Serialize(data)}");
            string resp = await _tcp.SendRequestAsync(ServerIp, request);

            try
            {
                var response = JsonSerializer.Deserialize<ServerResponse>(resp);
                if (response != null)
                {
                    string msg = string.IsNullOrEmpty(response.Message) ? "(no message)" : response.Message;
                    Debug.WriteLine($"[DEBUG] Ответ сервера: Status={response.Status}, Message={msg}");
                }
                else
                {
                    Debug.WriteLine("[ERROR] Некорректный ответ сервера");
                }

                return response;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERROR] Ошибка парсинга JSON ответа: {ex.Message}");
                return new ServerResponse { Status = "error", Message = "INVALID_JSON" };
            }
        }
    }
}