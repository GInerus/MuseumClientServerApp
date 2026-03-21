using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace MuseumServer.Network
{
    public class UdpServer
    {
        private readonly int port;

        public UdpServer(int port)
        {
            this.port = port;
        }

        public void Start()
        {
            Thread udpThread = new Thread(() => Run(port)); // передаём порт
            udpThread.Start();
        }

        private void Run(int udpPort)
        {
            using UdpClient udp = new UdpClient(udpPort);
            IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);

            Console.WriteLine($"UDP сервер слушает на порту {udpPort} для broadcast.");

            while (true)
            {
                try
                {
                    byte[] data = udp.Receive(ref remoteEP);
                    string msg = Encoding.UTF8.GetString(data);

                    if (msg == "DISCOVER_SERVER")
                    {
                        byte[] response = Encoding.UTF8.GetBytes("SERVER_HERE");
                        udp.Send(response, response.Length, remoteEP);
                        Console.WriteLine($"Ответил клиенту {remoteEP}");
                    }
                }
                catch (SocketException se)
                {
                    Console.WriteLine($"UDP ошибка: {se.Message}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"UDP неизвестная ошибка: {ex.Message}");
                }
            }
        }
    }
}