using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MeseumClient.Services
{
    public class TcpClientService
    {
        private readonly int _tcpPort = 9001;

        /// <summary>
        /// Отправляет JSON-запрос на сервер и возвращает JSON-ответ как строку.
        /// </summary>
        public async Task<string> SendRequestAsync(string serverIp, object request)
        {
            try
            {
                using TcpClient client = new TcpClient();
                await client.ConnectAsync(serverIp, _tcpPort);

                using NetworkStream stream = client.GetStream();
                using StreamWriter writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };
                using StreamReader reader = new StreamReader(stream, Encoding.UTF8);

                // Сериализация объекта запроса в JSON
                string json = JsonSerializer.Serialize(request);

                // Отправка запроса на сервер
                await writer.WriteLineAsync(json);

                // Чтение ответа (одна строка JSON)
                string response = await reader.ReadLineAsync();
                return response ?? "";
            }
            catch (Exception ex)
            {
                return $"ERROR: {ex.Message}";
            }
        }
    }
}