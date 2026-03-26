using System;
using System.Threading;
using MuseumServer.Network;
using MuseumServer.Services;

class Program
{
    static async Task Main()
    {
        SessionService sessionService = new SessionService();

        TcpServer tcpServer = new TcpServer(9001, sessionService);
        _ = tcpServer.Start(); // async

        while (true)
        {
            await Task.Delay(TimeSpan.FromMinutes(120));
            sessionService.CleanupOldSessions();
            Console.WriteLine("Старые сессии очищены");
        }
    }
}