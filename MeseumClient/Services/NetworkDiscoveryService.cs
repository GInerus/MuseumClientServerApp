using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MeseumClient.Services
{
    public class NetworkDiscoveryService
    {
        private const int UdpPort = 9000;

        public async Task<string?> DiscoverServerAsync(int timeoutMs = 3000)
        {
            using UdpClient udp = new UdpClient();
            udp.EnableBroadcast = true;
            var ep = new IPEndPoint(IPAddress.Broadcast, UdpPort);
            byte[] msg = Encoding.UTF8.GetBytes("DISCOVER_SERVER");
            await udp.SendAsync(msg, msg.Length, ep);

            var serverEP = new IPEndPoint(IPAddress.Any, 0);
            var task = udp.ReceiveAsync();

            if (await Task.WhenAny(task, Task.Delay(timeoutMs)) == task)
            {
                string respMsg = Encoding.UTF8.GetString(task.Result.Buffer);
                if (respMsg == "SERVER_HERE")
                    return task.Result.RemoteEndPoint.Address.ToString();
            }

            return null; // не найден
        }
    }
}
