using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MeseumClient.Services
{
    public class TcpClientService
    {
        private readonly int _tcpPort = 9001;

        public async Task<string> SendRequestAsync(string serverIp, string message)
        {
            try
            {
                using TcpClient client = new TcpClient();
                await client.ConnectAsync(serverIp, _tcpPort);

                using NetworkStream stream = client.GetStream();
                byte[] data = Encoding.UTF8.GetBytes(message);
                await stream.WriteAsync(data, 0, data.Length);

                byte[] buffer = new byte[4096];
                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                return Encoding.UTF8.GetString(buffer, 0, bytesRead);
            }
            catch (Exception ex)
            {
                return $"Ошибка: {ex.Message}";
            }
        }
    }
}
