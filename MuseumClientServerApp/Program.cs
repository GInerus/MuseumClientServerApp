using System;
using System.Threading;
using MuseumServer.Network;
using MuseumServer.Services;

class Program
{
    static void Main()
    {
        SessionService sessionService = new SessionService();

        // Запуск UDP для поиска сервера
        UdpServer udpServer = new UdpServer(9000);
        udpServer.Start();

        // Запуск TCP сервера
        TcpServer tcpServer = new TcpServer(9001, sessionService);
        Thread tcpThread = new Thread(tcpServer.Start);
        tcpThread.Start();

        // Очистка старых сессий каждые 2 часа
        while (true)
        {
            Thread.Sleep(TimeSpan.FromMinutes(120));
            sessionService.CleanupOldSessions(TimeSpan.FromMinutes(10));
            Console.WriteLine("Старые сессии очищены");
        }
    }
}