using System;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MeseumClient.Services
{
    public class TcpClientService
    {
        private readonly string _serverIp;
        private readonly int _tcpPort;
        private readonly int _connectTimeoutMs;

        //public TcpClientService(string serverIp = "192.168.0.150", int tcpPort = 9001, int connectTimeoutMs = 3000)
        public TcpClientService(string serverIp = "127.0.0.1", int tcpPort = 9001, int connectTimeoutMs = 3000)

        {
            _serverIp = serverIp;
            _tcpPort = tcpPort;
            _connectTimeoutMs = connectTimeoutMs;
        }

        public async Task<string> SendRequestAsync(object request)
        {
            try
            {
                using TcpClient client = new TcpClient();

                var connectTask = client.ConnectAsync(_serverIp, _tcpPort);
                if (await Task.WhenAny(connectTask, Task.Delay(_connectTimeoutMs)) != connectTask)
                    throw new TimeoutException($"Не удалось подключиться к {_serverIp}:{_tcpPort} за {_connectTimeoutMs} мс.");

                Debug.WriteLine($"[DEBUG] Подключено к серверу {_serverIp}:{_tcpPort}");

                using NetworkStream stream = client.GetStream();
                using StreamWriter writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };
                using StreamReader reader = new StreamReader(stream, Encoding.UTF8);

                string json = JsonSerializer.Serialize(request);
                await writer.WriteLineAsync(json);

                string response = await reader.ReadLineAsync();
                return response ?? "";
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERROR] TCP ошибка: {ex.Message}");
                return $"ERROR: {ex.Message}";
            }
        }
    }
}