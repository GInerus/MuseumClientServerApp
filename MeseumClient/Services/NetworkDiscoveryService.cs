using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
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

            byte[] msg = Encoding.UTF8.GetBytes("DISCOVER_SERVER");

            // получаем все broadcast адреса сети
            var broadcasts = GetAllBroadcastAddresses();

            // 🔁 отправляем несколько раз во все сети
            foreach (var b in broadcasts)
            {
                var ep = new IPEndPoint(b, UdpPort);

                for (int i = 0; i < 2; i++)
                {
                    await udp.SendAsync(msg, msg.Length, ep);
                    await Task.Delay(50); // небольшой интервал
                }
            }

            var timeout = Task.Delay(timeoutMs);

            while (true)
            {
                var receiveTask = udp.ReceiveAsync();

                // ждём либо ответа, либо таймаута
                var completed = await Task.WhenAny(receiveTask, timeout);

                if (completed == timeout)
                    break; // таймаут — сервер не найден

                var result = receiveTask.Result;
                string respMsg = Encoding.UTF8.GetString(result.Buffer);

                if (respMsg == "SERVER_HERE")
                {
                    var ip = result.RemoteEndPoint.Address;

                    // фильтруем loopback, чтобы не возвращать 127.0.0.1
                    if (!IPAddress.IsLoopback(ip))
                        return ip.ToString();
                }
            }

            return null; // сервер не найден
        }

        private static List<IPAddress> GetAllBroadcastAddresses()
        {
            var list = new List<IPAddress>();

            foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ni.OperationalStatus != OperationalStatus.Up)
                    continue;

                if (ni.NetworkInterfaceType == NetworkInterfaceType.Loopback)
                    continue;

                var props = ni.GetIPProperties();

                foreach (var ua in props.UnicastAddresses)
                {
                    if (ua.Address.AddressFamily != AddressFamily.InterNetwork)
                        continue;

                    if (ua.IPv4Mask == null)
                        continue;

                    var ip = ua.Address.GetAddressBytes();
                    var mask = ua.IPv4Mask.GetAddressBytes();

                    byte[] broadcast = new byte[4];

                    for (int i = 0; i < 4; i++)
                        broadcast[i] = (byte)(ip[i] | (mask[i] ^ 255));

                    list.Add(new IPAddress(broadcast));
                }
            }

            // fallback
            list.Add(IPAddress.Broadcast);

            return list;
        }
    }
}