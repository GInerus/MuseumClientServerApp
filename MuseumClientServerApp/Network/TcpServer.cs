using MuseumServer.Data;
using MuseumServer.Models;
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

        public async Task Start()
        {
            var listener = new TcpListener(IPAddress.Any, port);
            listener.Start();

            Console.WriteLine($"TCP сервер запущен на порту {port}");

            while (true)
            {
                var client = await listener.AcceptTcpClientAsync();
                _ = HandleClientAsync(client); // fire & forget
            }
        }

        private async Task HandleClientAsync(TcpClient client)
        {
            try
            {
                using var stream = client.GetStream();
                using var reader = new StreamReader(stream, Encoding.UTF8);
                using var writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };

                string requestJson = await reader.ReadLineAsync();

                if (string.IsNullOrEmpty(requestJson))
                    return;

                Console.WriteLine($"JSON запрос: {requestJson}");

                var request = JsonSerializer.Deserialize<Request>(requestJson);
                if (request == null || string.IsNullOrEmpty(request.Action))
                {
                    await SendError(writer, "INVALID_REQUEST");
                    return;
                }

                switch (request.Action)
                {
                    case "REGISTER_SESSION":
                        await HandleRegister(request, writer);
                        break;

                    case "GET_DEPARTMENTS":
                        await HandleDepartments(request, writer);
                        break;

                    case "GET_EXHIBITS":
                        await HandleExhibits(request, writer);
                        break;

                    case "GET_EXHIBIT":
                        await HandleExhibit(request, writer);
                        break;

                    default:
                        await SendError(writer, "UNKNOWN_ACTION");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }
            finally
            {
                client.Close();
            }
        }

        // ===== HANDLERS =====

        private async Task HandleRegister(Request request, StreamWriter writer)
        {
            string userType = request.Data?.GetProperty("userType").GetString() ?? "guest";
            string password = request.Data?.GetProperty("password").GetString() ?? "";

            if (userType == "admin" && password != "qwerty")
            {
                await SendError(writer, "ADMIN_AUTH_FAIL");
                return;
            }

            string token = sessionService.CreateSession(userType);
            await SendOk(writer, new { token });
            Console.WriteLine($"Создана сессия: {token} для {userType}");
        }

        private async Task HandleDepartments(Request request, StreamWriter writer)
        {
            if (!Validate(request.Token))
            {
                await SendError(writer, "INVALID_SESSION");
                return;
            }

            using var db = new MuseumContext();
            var departments = db.Departments
                .Select(d => new { d.DepartmentId, d.Name, d.Description })
                .ToList();

            await SendOk(writer, departments);
        }

        private async Task HandleExhibits(Request request, StreamWriter writer)
        {
            if (!Validate(request.Token))
            {
                await SendError(writer, "INVALID_SESSION");
                return;
            }

            if (!request.Data.HasValue ||
                !request.Data.Value.TryGetProperty("departmentId", out var deptProp) ||
                !deptProp.TryGetInt32(out int deptId))
            {
                await SendError(writer, "INVALID_DATA");
                return;
            }

            using var db = new MuseumContext();
            var exhibits = db.Exhibits
                .Where(e => e.DepartmentId == deptId)
                .Select(e => new { e.ExhibitId, e.Name, e.Description, e.ImagePath })
                .ToList();

            await SendOk(writer, exhibits);
        }

        private async Task HandleExhibit(Request request, StreamWriter writer)
        {
            if (!Validate(request.Token))
            {
                await SendError(writer, "INVALID_SESSION");
                return;
            }

            if (!request.Data.HasValue ||
                !request.Data.Value.TryGetProperty("exhibitId", out var exProp) ||
                !exProp.TryGetInt32(out int exId))
            {
                await SendError(writer, "INVALID_DATA");
                return;
            }

            using var db = new MuseumContext();
            var exhibit = db.Exhibits
                .Where(e => e.ExhibitId == exId)
                .Select(e => new { e.ExhibitId, e.Name, e.Description, e.ImagePath })
                .FirstOrDefault();

            await SendOk(writer, exhibit);
        }

        // ===== HELPERS =====

        private bool Validate(string token)
        {
            return !string.IsNullOrEmpty(token) && sessionService.ValidateSession(token);
        }

        private async Task SendOk(StreamWriter writer, object data)
        {
            var response = new Response { Status = "ok", Data = data };
            string json = JsonSerializer.Serialize(response);
            await writer.WriteLineAsync(json);
        }

        private async Task SendError(StreamWriter writer, string message)
        {
            var response = new Response { Status = "error", Message = message };
            string json = JsonSerializer.Serialize(response);
            await writer.WriteLineAsync(json);
        }
    }
}