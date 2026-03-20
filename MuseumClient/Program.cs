using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

class TestClient
{
    const int UdpPort = 9000;
    const int TcpPort = 9001;
    static string serverIp;
    static string sessionToken;

    static void Main()
    {
        Console.WriteLine("=== Тестовый клиент для Museum Server ===");

        // Поиск сервера
        DiscoverServer();
        if (serverIp == null)
        {
            Console.WriteLine("Сервер не найден!");
            return;
        }
        Console.WriteLine($"Сервер найден: {serverIp}");

        // Выбор пользователя
        Console.Write("Выберите тип пользователя (guest/admin): ");
        string userType = Console.ReadLine()?.Trim().ToLower() ?? "guest";
        string password = "";
        if (userType == "admin")
        {
            Console.Write("Введите пароль администратора: ");
            password = Console.ReadLine();
        }

        // Регистрация сессии
        sessionToken = RegisterSession(userType, password);
        if (string.IsNullOrEmpty(sessionToken))
        {
            Console.WriteLine("Не удалось создать сессию.");
            return;
        }
        Console.WriteLine($"Сессия создана. Токен: {sessionToken}");

        // Главное меню команд
        while (true)
        {
            Console.WriteLine("\nВыберите команду:");
            Console.WriteLine("1 - GET_DEPARTMENTS - Все отделы");
            Console.WriteLine("2 - GET_EXHIBITS (по DepartmentId) - Один отдел");
            Console.WriteLine("3 - GET_EXHIBIT (по ExhibitId) - Один экспонат");
            Console.WriteLine("0 - EXIT");

            string choice = Console.ReadLine()?.Trim();
            if (choice == "0") break;

            switch (choice)
            {
                case "1": // Departments
                    SendRequest($"GET_DEPARTMENTS|{sessionToken}");
                    break;

                case "2": // Exhibits
                    Console.Write("Введите DepartmentId: ");
                    string deptId = Console.ReadLine();
                    SendRequest($"GET_EXHIBITS|{sessionToken}|{deptId}");
                    break;

                case "3": // Один экспонат
                    Console.Write("Введите ExhibitId: ");
                    string exId = Console.ReadLine();
                    SendRequest($"GET_EXHIBIT|{sessionToken}|{exId}");
                    break;
            }
        }
    }

    static void DiscoverServer()
    {
        using UdpClient udp = new UdpClient();
        udp.EnableBroadcast = true;
        var ep = new IPEndPoint(IPAddress.Broadcast, UdpPort);

        byte[] msg = Encoding.UTF8.GetBytes("DISCOVER_SERVER");
        udp.Send(msg, msg.Length, ep);

        var serverEP = new IPEndPoint(IPAddress.Any, 0);
        udp.Client.ReceiveTimeout = 3000;

        try
        {
            byte[] response = udp.Receive(ref serverEP);
            string respMsg = Encoding.UTF8.GetString(response);
            if (respMsg == "SERVER_HERE")
                serverIp = serverEP.Address.ToString();
        }
        catch
        {
            serverIp = null;
        }
    }

    static string RegisterSession(string userType, string password)
    {
        string msg = userType == "admin"
            ? $"REGISTER_SESSION|admin|{password}"
            : $"REGISTER_SESSION|guest";

        string resp = SendTcpRequest(msg);
        if (resp == "ADMIN_AUTH_FAIL") return null;
        return resp;
    }

    static void SendRequest(string message)
    {
        string resp = SendTcpRequest(message); // message уже содержит токен
        if (!string.IsNullOrEmpty(resp))
        {
            Console.WriteLine("\nОтвет сервера (JSON):");
            Console.WriteLine(resp);
        }
    }

    static string SendTcpRequest(string message)
    {
        try
        {
            using TcpClient client = new TcpClient(serverIp, TcpPort);
            using NetworkStream stream = client.GetStream();
            byte[] data = Encoding.UTF8.GetBytes(message);
            stream.Write(data, 0, data.Length);

            byte[] buffer = new byte[4096]; // увеличили буфер для JSON
            int bytesRead = stream.Read(buffer, 0, buffer.Length);
            return Encoding.UTF8.GetString(buffer, 0, bytesRead);
        }
        catch (Exception ex)
        {
            return $"Ошибка: {ex.Message}";
        }
    }
}