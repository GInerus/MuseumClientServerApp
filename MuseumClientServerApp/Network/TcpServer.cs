using MuseumServer.Data;
using MuseumServer.Services;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;


namespace MuseumServer.Network
{
    public class TcpServer
    {
        private readonly int port;
        private readonly SessionService sessionService;

        public TcpServer(int port, SessionService sessionService)
        {
            this.port = port;
            this.sessionService = sessionService;
        }

        public void Start()
        {
            TcpListener listener = new TcpListener(IPAddress.Any, port);
            listener.Start();
            Console.WriteLine($"TCP сервер запущен на порту {port}");

            while (true)
            {
                var client = listener.AcceptTcpClient();
                ThreadPool.QueueUserWorkItem(HandleClient, client);
            }
        }

        private void HandleClient(object? clientObj)
        {
            var client = clientObj as TcpClient;
            if (client == null) return;

            using var stream = client.GetStream();
            byte[] buffer = new byte[4096]; // увеличенный буфер для JSON
            int bytesRead = stream.Read(buffer, 0, buffer.Length);
            string request = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            Console.WriteLine($"Получен запрос: {request}");

            string[] parts = request.Split('|');
            string action = parts[0];
            string response = "";
            string token = parts.Length > 1 ? parts[1] : null;
            Console.WriteLine($"Action: {action}, Token: {(token ?? "NULL")}");

            switch (action)
            {
                case "REGISTER_SESSION":
                    string userType = parts.Length > 1 ? parts[1] : "guest";
                    string password = parts.Length > 2 ? parts[2] : "";

                    if (userType == "admin" && password != "qwerty")
                    {
                        response = "ADMIN_AUTH_FAIL";
                    }
                    else
                    {
                        // Создаём сессию и возвращаем токен
                        string newToken = sessionService.CreateSession(userType);
                        response = newToken;
                        Console.WriteLine($"Создана сессия: {newToken} для {userType}");
                    }
                    break;

                case "GET_DEPARTMENTS":
                    if (!string.IsNullOrEmpty(token) && sessionService.ValidateSession(token))
                    {
                        using var db = new MuseumContext();
                        var departments = db.Departments
                            .Select(d => new {
                                d.DepartmentId,
                                d.Name,
                                d.Description
                            })
                            .ToList();

                        var options = new JsonSerializerOptions
                        {
                            WriteIndented = true,
                            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                        };

                        response = JsonSerializer.Serialize(departments, options);
                    }
                    else
                        response = "INVALID_SESSION";
                    break;

                case "GET_EXHIBITS":
                    if (!string.IsNullOrEmpty(token) && sessionService.ValidateSession(token) && parts.Length > 2)
                    {
                        int deptId = int.Parse(parts[2]);
                        using var db = new MuseumContext();
                        var exhibits = db.Exhibits
                            .Where(e => e.DepartmentId == deptId)
                            .Select(e => new {
                                e.ExhibitId,
                                e.Name,
                                e.Description,
                                e.ImagePath
                            })
                            .ToList();

                        var options = new JsonSerializerOptions
                        {
                            WriteIndented = true,
                            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                        };

                        response = JsonSerializer.Serialize(exhibits, options);
                    }
                    else
                        response = "INVALID_SESSION";
                    break;

                case "GET_EXHIBIT":
                    if (!string.IsNullOrEmpty(token) && sessionService.ValidateSession(token) && parts.Length > 2)
                    {
                        int exId = int.Parse(parts[2]);
                        using var db = new MuseumContext();
                        var exhibit = db.Exhibits
                            .Where(e => e.ExhibitId == exId)
                            .Select(e => new {
                                e.ExhibitId,
                                e.Name,
                                e.Description,
                                e.ImagePath
                            })
                            .FirstOrDefault();

                        var options = new JsonSerializerOptions
                        {
                            WriteIndented = true,
                            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                        };

                        response = exhibit != null ? JsonSerializer.Serialize(exhibit, options) : "{}";
                    }
                    else
                        response = "INVALID_SESSION";
                    break;

                default:
                    response = "UNKNOWN_ACTION";
                    break;
            }

            byte[] respBytes = Encoding.UTF8.GetBytes(response);
            stream.Write(respBytes, 0, respBytes.Length);
            client.Close();
        }
    }
}