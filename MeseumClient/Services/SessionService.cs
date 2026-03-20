using System.Text.Json;
using System.Threading.Tasks;

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
            ServerIp = await _discovery.DiscoverServerAsync();
            return ServerIp != null;
        }

        // Регистрация сессии (guest или admin)
        public async Task<bool> RegisterSessionAsync(string userType, string password = "")
        {
            if (ServerIp == null) return false;

            // Формируем объект запроса с обязательными полями action и data
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

            string resp = await _tcp.SendRequestAsync(ServerIp, request);

            try
            {
                var response = JsonSerializer.Deserialize<ServerResponse>(resp);
                if (response == null || response.Status != "ok") return false;

                Token = response.Data?.GetProperty("token").GetString();
                return Token != null;
            }
            catch
            {
                return false;
            }
        }

        // Общие команды к серверу
        public async Task<ServerResponse?> SendCommandAsync(string action, object? data = null)
        {
            if (ServerIp == null || Token == null)
                return new ServerResponse { Status = "error", Message = "NO_SESSION" };

            var request = new
            {
                action,
                token = Token,
                data
            };

            string resp = await _tcp.SendRequestAsync(ServerIp, request);

            try
            {
                var response = JsonSerializer.Deserialize<ServerResponse>(resp);
                return response;
            }
            catch
            {
                return new ServerResponse { Status = "error", Message = "INVALID_JSON" };
            }
        }

        // Модель для десериализации JSON-ответа
        public class ServerResponse
        {
            public string Status { get; set; } = "error";
            public JsonElement? Data { get; set; }
            public string Message { get; set; } = "";
        }
    }
}